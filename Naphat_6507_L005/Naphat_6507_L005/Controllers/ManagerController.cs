using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

// ฝ่ายผู้จัดการ: ดูภาพรวมธุรกิจ รายรับรายเดือน และสถานะห้อง (ดูอย่างเดียว ไม่แก้ข้อมูลในฐานข้อมูล)
public class ManagerController : Controller
{
    private readonly Csi402dbContext _db;

    // รับตัวเชื่อมฐานข้อมูลมาเก็บไว้ใน _db
    public ManagerController(Csi402dbContext db)
    {
        _db = db;
    }

    private static readonly CultureInfo _th = new("th-TH");

    // ชื่อขนาดห้องตาม room.SizeCat
    private static readonly Dictionary<string, string> _sizeText = new()
    {
        { "s", "S" },
        { "m", "M" },
        { "l", "L" },
        { "xl", "XL" },
        { "cold", "ห้องเย็น" },
    };

    private static readonly Dictionary<string, string> _roomStatusText = new()
    {
        { "Available", "ว่าง" },
        { "Booked", "ถูกจอง (รอชำระ/รอตรวจ)" },
        { "Occupied", "มีผู้เช่า" },
    };

    // ===================== ตัวช่วย =====================

    // เข้าได้เฉพาะผู้จัดการ
    private bool IsManager => HttpContext.Session.GetString("Role") == "Manager";

    // ส่งชื่อและ Role ผ่าน ViewBag ให้ Layout แสดง
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้จัดการ";
        ViewBag.Role = "Manager";
    }

    // ห้องทั้งหมด พร้อมการจองปัจจุบัน (รอชำระ / รอตรวจ / ใช้งานอยู่)
    private List<ManagerRoomViewModel> GetRooms()
    {
        var rooms = (from r in _db.Rooms orderby r.RoomId select r).ToList();
        var bookings = (from b in _db.Bookings
                        join u in _db.Users on b.UserId equals u.UserId
                        where b.BookingStatus == "Pending" || b.BookingStatus == "Checking" || b.BookingStatus == "Active"
                        orderby b.BookingDate descending
                        select new { b, u }).ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        List<ManagerRoomViewModel> list = new();
        foreach (var r in rooms)
        {
            ManagerRoomViewModel item = new();
            item.RoomId = r.RoomId;
            item.SizeText = _sizeText.ContainsKey(r.SizeCat) ? _sizeText[r.SizeCat] : r.SizeCat;
            item.Zone = r.Zone ?? "";
            item.Area = r.Size ?? 0;
            item.MonthlyRate = r.MonthlyRate;
            item.Status = r.Status;
            item.StatusText = _roomStatusText.ContainsKey(r.Status) ? _roomStatusText[r.Status] : r.Status;

            var current = (from x in bookings where x.b.RoomId == r.RoomId select x).FirstOrDefault();
            if (current != null)
            {
                item.BookingId = current.b.BookingId;
                item.TenantName = string.IsNullOrEmpty(current.u.CompanyName) ? current.u.Fullname : current.u.CompanyName;
                item.EndDateText = current.b.EndDate.ToString("d MMM yyyy", _th);
                item.DaysLeft = current.b.EndDate.DayNumber - today.DayNumber;
            }
            list.Add(item);
        }
        return list;
    }

    // สรุปห้องตามขนาด เรียง S, M, L, XL, ห้องเย็น
    private static List<RoomSizeSummaryViewModel> SummarizeBySize(List<ManagerRoomViewModel> rooms)
    {
        var order = _sizeText.Values.ToList();
        return (from r in rooms
                group r by r.SizeText into g
                orderby order.IndexOf(g.Key)
                select new RoomSizeSummaryViewModel
                {
                    SizeText = g.Key,
                    Total = g.Count(),
                    Available = g.Count(x => x.Status == "Available"),
                    Booked = g.Count(x => x.Status == "Booked"),
                    Occupied = g.Count(x => x.Status == "Occupied"),
                }).ToList();
    }

    // ===================== หน้า =====================

    // ภาพรวมธุรกิจ
    // GET รวมรายรับรายเดือนด้วย GetMonthlyRevenue นับห้องตามสถานะ และนับงานค้างของแต่ละฝ่าย แล้วส่งให้ View
    public IActionResult Dashboard()
    {
        if (!IsManager)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        CustomerController.CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล

        // รายรับ 6 เดือนล่าสุด (เดือนสุดท้าย = เดือนนี้)
        var revenue = AccountingController.GetMonthlyRevenue(_db, 6);
        var thisMonth = revenue[revenue.Count - 1];
        var lastMonth = revenue[revenue.Count - 2];
        ViewBag.Revenue = revenue;
        ViewBag.RentThisMonth = thisMonth.Rent;
        ViewBag.RentChangeText = lastMonth.Rent == 0
            ? (thisMonth.Rent == 0 ? "เท่ากับเดือนก่อน" : "เดือนก่อนไม่มีรายรับ")
            : ((thisMonth.Rent - lastMonth.Rent) * 100 / lastMonth.Rent).ToString("+0;-0;0") + "% จากเดือนก่อน";

        // ห้อง
        var rooms = GetRooms();
        var usedCount = rooms.Count(r => r.Status != "Available");
        ViewBag.RoomTotal = rooms.Count;
        ViewBag.RoomUsed = usedCount;
        ViewBag.UsedPercent = rooms.Count == 0 ? 0 : (int)Math.Round(usedCount * 100.0 / rooms.Count);
        ViewBag.SizeSummary = SummarizeBySize(rooms);

        // การจองใหม่เดือนนี้ (ไม่นับที่ยกเลิก) และผู้เช่าที่ใช้งานอยู่
        var monthStart = thisMonth.Month;
        ViewBag.NewBookings = (from b in _db.Bookings
                               where b.BookingDate >= monthStart && b.BookingStatus != "Cancelled"
                               select b).Count();
        ViewBag.ActiveTenants = (from b in _db.Bookings where b.BookingStatus == "Active" select b.UserId).Distinct().Count();

        // งานค้างของแต่ละฝ่าย
        List<PendingWorkViewModel> work = new();
        void AddWork(string dept, string icon, string color, string text, int count)
        {
            PendingWorkViewModel w = new();
            w.Department = dept;
            w.Icon = icon;
            w.Color = color;
            w.Text = text;
            w.Count = count;
            work.Add(w);
        }
        AddWork("ลูกค้า", "hourglass_top", "#3b82f6", "การจองรอลูกค้าชำระเงิน",
                (from b in _db.Bookings where b.BookingStatus == "Pending" select b).Count());
        AddWork("บัญชี", "receipt_long", "#10b981", "การชำระรอฝ่ายบัญชีตรวจ",
                (from p in _db.Payments where p.PaymentStatus == "Pending" select p).Count());
        AddWork("คลังสินค้า", "inventory", "#8b5cf6", "การจองรอตรวจรับสินค้า",
                (from s in _db.Stockitems
                 join e in _db.Users on s.UpdatedBy equals e.UserId
                 join b in _db.Bookings on s.BookingId equals b.BookingId
                 where e.Role == "Customer" && b.BookingStatus == "Active"
                 select b.BookingId).Distinct().Count());
        AddWork("คลังสินค้า", "warning", "#f59e0b", "สินค้าใกล้หมด",
                (from s in _db.Stockitems
                 join e in _db.Users on s.UpdatedBy equals e.UserId
                 where e.Role != "Customer" && s.Quantity <= s.MinQuantity
                 select s).Count());
        AddWork("บริการลูกค้า", "support_agent", "#06b6d4", "ตั๋วที่ยังไม่มีคนรับเรื่อง",
                (from t in _db.Supporttickets where t.TicketStatus == "Open" select t).Count());
        AddWork("บริการลูกค้า", "priority_high", "#ef4444", "ตั๋วด่วนที่ยังไม่ปิด",
                (from t in _db.Supporttickets where t.Priority == "High" && t.TicketStatus != "Resolved" select t).Count());
        ViewBag.Work = work;

        // สัญญาที่จะหมดภายใน 30 วัน
        return View((from r in rooms
                     where r.Status == "Occupied" && r.DaysLeft != null && r.DaysLeft <= 30
                     orderby r.DaysLeft
                     select r).ToList());
    }

    // รายงานรายรับรายเดือน (ย้อนหลัง 12 เดือน)
    // GET เรียก AccountingController.GetMonthlyRevenue ใช้ฟังก์ชันเดียวกับฝ่ายบัญชี
    public IActionResult Revenue()
    {
        if (!IsManager)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var months = AccountingController.GetMonthlyRevenue(_db, 12);
        ViewBag.TotalRent = months.Sum(m => m.Rent);
        ViewBag.TotalDeposit = months.Sum(m => m.Deposit);
        ViewBag.TotalRefund = months.Sum(m => m.Refund);
        ViewBag.TotalNet = months.Sum(m => m.Net);
        ViewBag.TotalCount = months.Sum(m => m.Count);
        ViewBag.RangeText = months[0].MonthText + " – " + months[months.Count - 1].MonthText;

        // ล่าสุดอยู่บน
        months.Reverse();
        return View(months);
    }

    // สถานะห้องทั้งหมด กรองตามสถานะได้ (?status=Available / Booked / Occupied)
    // GET รับ status อ่านห้องทั้งหมดพร้อมผู้เช่า แล้วสรุปตามขนาด
    public IActionResult Rooms(string? status)
    {
        if (!IsManager)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        CustomerController.CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล

        var rooms = GetRooms();
        ViewBag.SizeSummary = SummarizeBySize(rooms);
        ViewBag.CountAll = rooms.Count;
        ViewBag.CountAvailable = rooms.Count(r => r.Status == "Available");
        ViewBag.CountBooked = rooms.Count(r => r.Status == "Booked");
        ViewBag.CountOccupied = rooms.Count(r => r.Status == "Occupied");

        if (status != "Available" && status != "Booked" && status != "Occupied")
        {
            status = null;
        }
        ViewBag.Status = status;
        return View((from r in rooms where status == null || r.Status == status select r).ToList());
    }
}
