using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class InventoryController : Controller
{
    private readonly Csi402dbContext _db;

    public InventoryController(Csi402dbContext db)
    {
        _db = db;
    }

    private static readonly CultureInfo _th = new("th-TH");

    // ===================== ตัวช่วย =====================

    // เข้าได้เฉพาะพนักงานคลัง
    private bool IsInventory => HttpContext.Session.GetString("Role") == "Inventory";

    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "พนักงานคลังสินค้า";
        ViewBag.Role = "Inventory";

        // จำนวนสินค้าใกล้หมด (เฉพาะที่ตรวจรับแล้ว) และการจองที่รอตรวจรับ: _Layout ใช้แสดงป้ายตัวเลขที่เมนูและแถบเตือนด้านบน
        ViewBag.LowStockCount = (from s in _db.Stockitems
                                 join e in _db.Users on s.UpdatedBy equals e.UserId
                                 where e.Role != "Customer" && s.Quantity <= s.MinQuantity
                                 select s).Count();
        ViewBag.PendingReceiveCount = GetReceiveBookings().Count(b => b.CanReceive);
    }

    // พนักงานที่ล็อกอินอยู่ (ใช้บันทึก UpdatedBy)
    private User? GetCurrentStaff()
    {
        var username = HttpContext.Session.GetString("Username");
        return (from u in _db.Users where u.Username == username && u.Role == "Inventory" select u).FirstOrDefault();
    }

    // ดึงสินค้า + การจอง + ลูกค้า + คนแก้ล่าสุด แล้วแปลงเป็น ViewModel (ใช้ร่วมกับแท็บสต็อกของลูกค้า)
    public static List<StockItemViewModel> GetStockItems(Csi402dbContext db, string? bookingId, bool lowOnly, int? userId)
    {
        var rows = (from s in db.Stockitems
                    join b in db.Bookings on s.BookingId equals b.BookingId
                    join u in db.Users on b.UserId equals u.UserId
                    join e in db.Users on s.UpdatedBy equals e.UserId
                    where (bookingId == null || s.BookingId == bookingId)
                          && (!lowOnly || (s.Quantity <= s.MinQuantity && e.Role != "Customer"))
                          && (userId == null || b.UserId == userId)
                    orderby b.RoomId, s.ItemName
                    select new { s, b, u, e }).ToList();

        List<StockItemViewModel> list = new();
        foreach (var row in rows)
        {
            StockItemViewModel item = new();
            item.StockId = row.s.StockId;
            item.BookingId = row.b.BookingId;
            item.RoomId = row.b.RoomId;
            item.CustomerName = string.IsNullOrEmpty(row.u.CompanyName) ? row.u.Fullname : row.u.CompanyName;
            item.ItemName = row.s.ItemName;
            item.Category = row.s.Category;
            item.Quantity = row.s.Quantity;
            item.MinQuantity = row.s.MinQuantity;
            item.Unit = row.s.Unit;
            item.UpdatedText = row.s.LastUpdated.ToString("d MMM yyyy HH:mm", _th);
            item.UpdatedByName = row.e.Fullname;
            item.IsVerified = row.e.Role != "Customer";
            list.Add(item);
        }
        return list;
    }

    // การจองที่ใช้งานอยู่ (เก็บสินค้าได้) สำหรับช่องเลือก/กรอง
    private void SetActiveBookingsBag()
    {
        ViewBag.ActiveBookings = (from b in _db.Bookings
                                  join u in _db.Users on b.UserId equals u.UserId
                                  where b.BookingStatus == "Active"
                                  orderby b.RoomId
                                  select new { b.BookingId, b.RoomId, Name = u.CompanyName ?? u.Fullname })
                                 .ToList()
                                 .Select(x => (x.BookingId, Text: x.RoomId + " · " + x.BookingId + " · " + x.Name))
                                 .ToList();
    }

    // ===================== หน้า =====================

    // ภาพรวม: ตัวเลขสรุป + สินค้าใกล้หมด 5 อันดับ
    public IActionResult Dashboard()
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        // นับเฉพาะสินค้าที่ตรวจรับแล้ว (คนแก้ล่าสุดไม่ใช่ลูกค้า)
        ViewBag.ItemCount = (from s in _db.Stockitems
                             join e in _db.Users on s.UpdatedBy equals e.UserId
                             where e.Role != "Customer"
                             select s).Count();
        ViewBag.LowCount = ViewBag.LowStockCount;
        ViewBag.UpdatedToday = (from s in _db.Stockitems where s.LastUpdated >= today && s.LastUpdated < tomorrow select s).Count();

        var low = GetStockItems(_db, null, true, null)
                  .OrderBy(s => s.Quantity - s.MinQuantity)
                  .Take(5)
                  .ToList();
        return View(low);
    }

    // รายการสต็อกทั้งหมด (กรองตามการจองได้)
    public IActionResult StockList(string? bookingId)
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        SetActiveBookingsBag();
        ViewBag.FilterBookingId = bookingId;
        return View(GetStockItems(_db, string.IsNullOrEmpty(bookingId) ? null : bookingId, false, null));
    }

    // สินค้าที่เหลือเท่ากับหรือน้อยกว่าจำนวนขั้นต่ำ
    public IActionResult Alerts()
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        return View(GetStockItems(_db, null, true, null));
    }

    // หน้าเพิ่ม / แก้ไขสินค้า (GET) id = null คือเพิ่มใหม่ - แบบเดียวกับ Admin/EditUser
    [HttpGet]
    public IActionResult AddStock(int? id, string? bookingId)
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        SetActiveBookingsBag();

        StockFormViewModel data = new();
        if (id != null)
        {
            var stock = (from s in _db.Stockitems where s.StockId == id select s).FirstOrDefault();
            if (stock == null)
            {
                TempData["Error"] = "ไม่พบสินค้านี้";
                return RedirectToAction("StockList");
            }
            data.StockId = stock.StockId;
            data.BookingId = stock.BookingId;
            data.ItemName = stock.ItemName;
            data.Category = stock.Category;
            data.Unit = stock.Unit;
            data.MinQuantity = stock.MinQuantity;
            data.Quantity = stock.Quantity;
        }
        else
        {
            data.BookingId = bookingId ?? "";
        }
        return View(data);
    }

    // บันทึกการเพิ่ม / แก้ไขสินค้า (POST)
    [HttpPost]
    public IActionResult AddStock(StockFormViewModel data)
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        SetActiveBookingsBag();

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            ViewBag.Error = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานคลังในฐานข้อมูล (เช่น inv01)";
            return View(data);
        }

        var bookingOk = (from b in _db.Bookings where b.BookingId == data.BookingId && b.BookingStatus == "Active" select b).Any();
        if (!bookingOk)
        {
            ViewBag.Error = "กรุณาเลือกการจองที่ใช้งานอยู่";
            return View(data);
        }

        if (string.IsNullOrWhiteSpace(data.ItemName))
        {
            ViewBag.Error = "กรุณากรอกชื่อสินค้า";
            return View(data);
        }

        if (data.Quantity < 0 || data.MinQuantity < 0)
        {
            ViewBag.Error = "จำนวนต้องไม่ติดลบ";
            return View(data);
        }

        Stockitem? stock;
        if (data.StockId == 0)
        {
            stock = new Stockitem();
            _db.Stockitems.Add(stock);
        }
        else
        {
            stock = (from s in _db.Stockitems where s.StockId == data.StockId select s).FirstOrDefault();
            if (stock == null)
            {
                TempData["Error"] = "ไม่พบสินค้านี้";
                return RedirectToAction("StockList");
            }
        }

        stock.BookingId = data.BookingId;
        stock.ItemName = data.ItemName.Trim();
        stock.Category = string.IsNullOrWhiteSpace(data.Category) ? null : data.Category.Trim();
        stock.Unit = string.IsNullOrWhiteSpace(data.Unit) ? null : data.Unit.Trim();
        stock.MinQuantity = data.MinQuantity;
        stock.Quantity = data.Quantity;
        stock.LastUpdated = DateTime.Now;
        stock.UpdatedBy = staff.UserId;
        _db.SaveChanges();

        TempData["Success"] = (data.StockId == 0 ? "เพิ่มสินค้า " : "แก้ไขสินค้า ") + stock.ItemName + " แล้ว";
        return RedirectToAction("StockList", new { bookingId = stock.BookingId });
    }

    // รับเข้า / เบิกออก (POST)
    [HttpPost]
    public IActionResult AdjustStock(StockAdjustViewModel data)
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }

        IActionResult Back()
        {
            if (data.ReturnTo == "Alerts")
            {
                return RedirectToAction("Alerts");
            }
            return RedirectToAction("StockList", new { bookingId = data.FilterBookingId });
        }

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            TempData["Error"] = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานคลังในฐานข้อมูล (เช่น inv01)";
            return Back();
        }

        var stock = (from s in _db.Stockitems where s.StockId == data.StockId select s).FirstOrDefault();
        if (stock == null)
        {
            TempData["Error"] = "ไม่พบสินค้านี้";
            return Back();
        }

        if (data.Amount <= 0)
        {
            TempData["Error"] = "กรุณากรอกจำนวนมากกว่า 0";
            return Back();
        }

        var unit = stock.Unit ?? "";
        if (data.Direction == "out")
        {
            if (data.Amount > stock.Quantity)
            {
                TempData["Error"] = "เบิก " + stock.ItemName + " ได้ไม่เกิน " + stock.Quantity + " " + unit;
                return Back();
            }
            stock.Quantity -= data.Amount;
        }
        else if (data.Direction == "in")
        {
            stock.Quantity += data.Amount;
        }
        else
        {
            TempData["Error"] = "รายการไม่ถูกต้อง";
            return Back();
        }

        stock.LastUpdated = DateTime.Now;
        stock.UpdatedBy = staff.UserId;
        _db.SaveChanges();

        TempData["Success"] = (data.Direction == "in" ? "รับเข้า " : "เบิกออก ") + stock.ItemName + " " + data.Amount + " " + unit
                              + " เหลือ " + stock.Quantity + " " + unit;
        return Back();
    }

    // ===================== ตรวจรับสินค้า =====================

    // การจองที่มีสินค้าที่ลูกค้าแจ้งไว้แต่ยังไม่ตรวจรับ (ไม่รวมการจองที่สิ้นสุด/ยกเลิก)
    private List<ReceiveBookingViewModel> GetReceiveBookings()
    {
        var rows = (from s in _db.Stockitems
                    join e in _db.Users on s.UpdatedBy equals e.UserId
                    join b in _db.Bookings on s.BookingId equals b.BookingId
                    join u in _db.Users on b.UserId equals u.UserId
                    where e.Role == "Customer" && b.BookingStatus != "Ended" && b.BookingStatus != "Cancelled"
                    select new { b.BookingId, b.RoomId, b.StartDate, b.BookingStatus, u.CompanyName, u.Fullname, u.Phone })
                   .ToList();

        return (from r in rows
                group r by r.BookingId into g
                let first = g.First()
                orderby first.StartDate
                select new ReceiveBookingViewModel
                {
                    BookingId = first.BookingId,
                    RoomId = first.RoomId,
                    CustomerName = string.IsNullOrEmpty(first.CompanyName) ? first.Fullname : first.CompanyName,
                    Phone = first.Phone,
                    StartDateText = first.StartDate.ToString("d MMM yyyy", _th),
                    BookingStatus = first.BookingStatus,
                    ItemCount = g.Count()
                }).ToList();
    }

    // รายการการจองที่รอตรวจรับ
    public IActionResult Receive()
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        return View(GetReceiveBookings());
    }

    // หน้าตรวจรับสินค้าของ 1 การจอง (GET): จำนวนที่ลูกค้าแจ้ง + ช่องกรอกจำนวนที่นับได้จริง
    [HttpGet]
    public IActionResult ReceiveItems(string id)
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var booking = GetReceiveBookings().FirstOrDefault(b => b.BookingId == id);
        if (booking == null)
        {
            TempData["Error"] = "การจองนี้ไม่มีสินค้ารอตรวจรับ";
            return RedirectToAction("Receive");
        }
        ViewBag.Booking = booking;

        ReceiveFormViewModel data = new();
        data.BookingId = booking.BookingId;
        data.Items = (from s in _db.Stockitems
                      join e in _db.Users on s.UpdatedBy equals e.UserId
                      where s.BookingId == id && e.Role == "Customer"
                      orderby s.StockId
                      select new ReceiveItemViewModel
                      {
                          StockId = s.StockId,
                          ItemName = s.ItemName,
                          Unit = s.Unit,
                          Declared = s.Quantity,
                          Counted = s.Quantity,
                          MinQuantity = s.MinQuantity
                      }).ToList();
        return View(data);
    }

    // ยืนยันตรวจรับ (POST): บันทึกจำนวนจริง, คนตรวจรับเป็นพนักงาน, ถ้าไม่ตรงกับที่แจ้งจดไว้ใน booking.Note
    [HttpPost]
    public IActionResult ReceiveItems(ReceiveFormViewModel data)
    {
        if (!IsInventory)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var booking = GetReceiveBookings().FirstOrDefault(b => b.BookingId == data.BookingId);
        if (booking == null)
        {
            TempData["Error"] = "การจองนี้ไม่มีสินค้ารอตรวจรับ";
            return RedirectToAction("Receive");
        }
        ViewBag.Booking = booking;

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            ViewBag.Error = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานคลังในฐานข้อมูล (เช่น inv01)";
            return View(data);
        }
        if (!booking.CanReceive)
        {
            ViewBag.Error = "ตรวจรับได้หลังลูกค้าชำระเงินและการจองใช้งานแล้ว";
            return View(data);
        }
        if (data.Items.Any(i => i.Counted < 0 || i.MinQuantity < 0))
        {
            ViewBag.Error = "จำนวนต้องไม่ติดลบ";
            return View(data);
        }

        // สินค้าที่รอตรวจรับของการจองนี้ (ไม่เชื่อ StockId ที่ส่งมาตรงๆ ต้องเป็นของการจองนี้เท่านั้น)
        var pending = (from s in _db.Stockitems
                       join e in _db.Users on s.UpdatedBy equals e.UserId
                       where s.BookingId == data.BookingId && e.Role == "Customer"
                       select s).ToList();

        List<string> mismatches = new();
        foreach (var input in data.Items)
        {
            var stock = pending.FirstOrDefault(s => s.StockId == input.StockId);
            if (stock == null)
            {
                continue;
            }
            if (input.Counted != stock.Quantity)
            {
                mismatches.Add(stock.ItemName + " แจ้ง " + stock.Quantity + " นับได้ " + input.Counted + " " + (stock.Unit ?? ""));
            }
            stock.Quantity = input.Counted;
            stock.MinQuantity = input.MinQuantity;
            stock.LastUpdated = DateTime.Now;
            stock.UpdatedBy = staff.UserId;
        }

        // ถ้าจำนวนไม่ตรงกับที่ลูกค้าแจ้ง จดไว้ต่อท้ายหมายเหตุการจอง
        if (mismatches.Count > 0)
        {
            var b = (from x in _db.Bookings where x.BookingId == data.BookingId select x).First();
            var line = "[ตรวจรับ " + DateTime.Now.ToString("d MMM yyyy", _th) + "] ไม่ตรงกับที่แจ้ง: " + string.Join(", ", mismatches);
            b.Note = string.IsNullOrEmpty(b.Note) ? line : b.Note + "\n" + line;
        }
        _db.SaveChanges();

        TempData["Success"] = "ตรวจรับสินค้าการจอง " + data.BookingId + " แล้ว"
                              + (mismatches.Count > 0 ? " (ไม่ตรงกับที่แจ้ง " + mismatches.Count + " รายการ บันทึกไว้ในหมายเหตุการจอง)" : " ครบตามที่แจ้ง");
        return RedirectToAction("StockList", new { bookingId = data.BookingId });
    }
}
