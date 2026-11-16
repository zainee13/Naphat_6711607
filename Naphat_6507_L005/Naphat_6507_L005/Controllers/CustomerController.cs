using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;
using static Naphat_6507_L005.Controllers.AccountingController;

namespace Naphat_6507_L005.Controllers;

public class CustomerController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly IWebHostEnvironment _env;

    public CustomerController(Csi402dbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // ===================== ข้อมูลเสริมของห้อง =====================
    // ตาราง room ยังไม่มีคอลัมน์เหล่านี้ (ตำแหน่งบนผัง, สเปก, ราคาเดิม) จึงเก็บไว้ที่นี่ก่อน
    // ข้อมูลหลักของห้อง (ขนาด, ราคา, สถานะ, รูป) ดึงจากตาราง room
    private static readonly Dictionary<string, RoomViewModel> _roomExtras = new()
    {
        { "XL-01", new RoomViewModel { Grid = "1 / 1 / 6 / 6",   Vehicle = "คอนเทนเนอร์ทุกขนาด",       Temp = "อุณหภูมิปกติ",   FloorLoad = "5 ตัน/ตร.ม.",   Dock = "Dock Leveler 4 ช่อง",     Power = "หม้อแปลงแยก 250 kVA", Note = "คลังโล่งขนาดใหญ่พิเศษ",                   OldPrice = 210000 } },
        { "L-01",  new RoomViewModel { Grid = "1 / 6 / 4 / 10",  Vehicle = "รถพ่วง 18 ล้อ / ตู้ 40ft", Temp = "อุณหภูมิปกติ",   FloorLoad = "5 ตัน/ตร.ม.",   Dock = "Dock Leveler 2 ช่อง",     Power = "3 เฟส 380V (200A)",   Note = "มี Dock Leveler เทียบตู้คอนเทนเนอร์",     OldPrice = 85000 } },
        { "L-02",  new RoomViewModel { Grid = "1 / 10 / 4 / 13", Vehicle = "10 ล้อ / เทรลเลอร์",       Temp = "อุณหภูมิปกติ",   FloorLoad = "4 ตัน/ตร.ม.",   Dock = "Dock Leveler 1 ช่อง",     Power = "3 เฟส 380V (150A)",   Note = "ควบคุมการเข้าออกเฉพาะบุคคล",              OldPrice = 0 } },
        { "M-02",  new RoomViewModel { Grid = "4 / 6 / 7 / 9",   Vehicle = "10 ล้อ",                   Temp = "อุณหภูมิปกติ",   FloorLoad = "3.5 ตัน/ตร.ม.", Dock = "ชานชาลาสูง 1.2 ม.",       Power = "3 เฟส 380V (100A)",   Note = "Racking 3 ชั้น วางได้ 120 พาเลท",          OldPrice = 0 } },
        { "M-01",  new RoomViewModel { Grid = "4 / 9 / 7 / 11",  Vehicle = "6 ล้อ / 10 ล้อ",           Temp = "อุณหภูมิปกติ",   FloorLoad = "3 ตัน/ตร.ม.",   Dock = "ชานชาลาพร้อมทางลาด",      Power = "3 เฟส 380V (50A)",    Note = "พื้นอีพ็อกซี่รับน้ำหนัก 3 ตัน/ตร.ม.",      OldPrice = 29000 } },
        { "CR-05", new RoomViewModel { Grid = "4 / 11 / 7 / 13", Vehicle = "10 ล้อตู้เย็น",             Temp = "−18°C ถึง +5°C", FloorLoad = "3 ตัน/ตร.ม.",   Dock = "Air Lock Loading Bay",    Power = "3 เฟส + ไฟสำรอง",     Note = "ควบคุม −18°C ถึง +5°C ดูอุณหภูมิผ่านมือถือ", OldPrice = 48000 } },
        { "S-01",  new RoomViewModel { Grid = "6 / 1 / 7 / 3",   Vehicle = "กระบะ / 4 ล้อ",            Temp = "อุณหภูมิปกติ",   FloorLoad = "1.5 ตัน/ตร.ม.", Dock = "ทางลาดเทียบรถกระบะ",      Power = "220V 1 เฟส",          Note = "ชั้น 1 เข้าถึงง่าย",                       OldPrice = 0 } },
        { "S-02",  new RoomViewModel { Grid = "6 / 3 / 7 / 6",   Vehicle = "4 ล้อ / 6 ล้อ",            Temp = "อุณหภูมิปกติ",   FloorLoad = "2 ตัน/ตร.ม.",   Dock = "ทางลาดโหลดสินค้าในร่ม",   Power = "220V / 380V",         Note = "ประตูม้วนไฟฟ้า",                           OldPrice = 18000 } },
    };

    // รหัสห้องแบบเก่า (บางหน้ายังลิงก์ด้วยรหัสนี้) แปลงเป็นรหัสใหม่
    private static readonly Dictionary<string, string> _oldRoomIds = new()
    {
        { "BN-A01", "M-01" },
        { "WN-B12", "L-01" },
        { "LC-C05", "CR-05" },
        { "LK-D03", "L-02" },
        { "BN-A07", "S-02" },
        { "WN-E01", "XL-01" },
    };

    // ชื่อวิธีชำระเงินที่แสดงในประวัติ
    public static readonly Dictionary<string, string> _payMethodText = new()
    {
        { "transfer", "โอนผ่าน SCB" },
        { "qr", "พร้อมเพย์ QR" },
        { "card", "บัตรเครดิต / เดบิต" },
    };

    private static readonly CultureInfo _th = new("th-TH");

    // ===================== ตัวช่วย =====================

    private bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    // หน้าลูกค้าใช้ได้เฉพาะลูกค้ากับ Admin เว็บ พนักงานฝ่ายอื่น (บัญชี / คลัง / CS / ผู้จัดการ) ใช้ไม่ได้
    private string? StaffRole => HttpContext.Session.GetString("Role");
    private bool IsStaff => IsLoggedIn && StaffRole != "Customer" && StaffRole != "Admin";

    private void SetUserBag()
    {
        var username = HttpContext.Session.GetString("Username");
        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(username);
        ViewBag.Username = username;
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้เยี่ยมชม";
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "Guest";
    }

    // ===== เบอร์โทร / เลขผู้เสียภาษี =====
    // ฐานข้อมูลเก็บตัวเลขล้วน ส่วนช่องกรอกในหน้าเว็บแสดงแบบเว้นวรรค (096 938 2439, 1 2345 67890 12 3)
    // ลูกค้าจะพิมพ์ติดกันหรือเว้นวรรค/มีขีดก็ได้ Controller ตัดเหลือแต่ตัวเลขก่อนตรวจและบันทึก

    // เหลือแต่ตัวเลข
    public static string? DigitsOnly(string? value)
    {
        return value == null ? null : new string(value.Where(char.IsDigit).ToArray());
    }

    // แบ่งตัวเลขเป็นกลุ่มแล้วคั่นด้วยเว้นวรรค ถ้าจำนวนหลักไม่ตรงรูปแบบ คืนค่าเดิม
    private static string GroupDigits(string digits, int[] sizes)
    {
        if (digits.Length != sizes.Sum())
        {
            return digits;
        }
        List<string> parts = new();
        int start = 0;
        foreach (var size in sizes)
        {
            parts.Add(digits.Substring(start, size));
            start += size;
        }
        return string.Join(" ", parts);
    }

    // เบอร์มือถือ 10 หลัก: 096 938 2439, เบอร์บ้าน 9 หลัก: 02 123 4567
    public static string? FormatPhone(string? phone)
    {
        var digits = DigitsOnly(phone);
        if (string.IsNullOrEmpty(digits))
        {
            return phone;
        }
        return digits.Length == 9 ? GroupDigits(digits, new[] { 2, 3, 4 }) : GroupDigits(digits, new[] { 3, 3, 4 });
    }

    // เลขผู้เสียภาษี / เลขบัตรประชาชน 13 หลัก: 1 2345 67890 12 3
    public static string? FormatTaxId(string? taxId)
    {
        var digits = DigitsOnly(taxId);
        return string.IsNullOrEmpty(digits) ? taxId : GroupDigits(digits, new[] { 1, 4, 5, 2, 1 });
    }

    // ผู้ใช้ที่ล็อกอินอยู่ (จากตาราง user) บัญชีพนักงานตัวอย่างจะไม่มีในตาราง ได้ null
    private User? GetCurrentUser()
    {
        var username = HttpContext.Session.GetString("Username");
        return (from u in _db.Users where u.Username == username select u).FirstOrDefault();
    }

    // แปลงข้อมูลจากตาราง room เป็น RoomViewModel แล้วเติมข้อมูลเสริม
    private RoomViewModel ToRoomViewModel(Room r, string? rating)
    {
        RoomViewModel room = new();
        room.Id = r.RoomId;
        room.Kind = r.SizeCat;
        room.Free = r.Status == "Available";
        room.Area = (r.Size ?? 0).ToString("N0");
        room.Height = r.Height ?? "-";
        room.Zone = r.Zone ?? "";
        room.Desc = r.Description ?? "";
        room.Price = (int)r.MonthlyRate;
        room.Img = r.ImageUrl ?? "";
        room.Rating = rating;

        if (_roomExtras.ContainsKey(r.RoomId))
        {
            var extra = _roomExtras[r.RoomId];
            room.Grid = extra.Grid;
            room.Vehicle = extra.Vehicle;
            room.Temp = extra.Temp;
            room.FloorLoad = extra.FloorLoad;
            room.Dock = extra.Dock;
            room.Power = extra.Power;
            room.Note = extra.Note;
            room.OldPrice = extra.OldPrice;
        }
        else
        {
            room.Grid = "auto";
            room.Vehicle = room.Temp = room.FloorLoad = room.Dock = room.Power = "-";
            room.Note = "";
        }
        return room;
    }

    // คะแนนเฉลี่ยรีวิวของห้อง (null = ยังไม่มีรีวิว)
    private string? GetRating(string roomId)
    {
        var stars = (from rv in _db.Reviews
                     join b in _db.Bookings on rv.BookingId equals b.BookingId
                     where b.RoomId == roomId
                     select rv.OverallRating).ToList();
        return stars.Count == 0 ? null : stars.Average().ToString("0.0");
    }

    // หาห้องตามรหัส (รองรับรหัสเก่า) ถ้าไม่เจอให้ใช้ M-01
    private Room? FindRoom(string? id)
    {
        id ??= "M-01";
        if (_oldRoomIds.ContainsKey(id))
        {
            id = _oldRoomIds[id];
        }
        var room = (from r in _db.Rooms where r.RoomId == id select r).FirstOrDefault();
        return room ?? (from r in _db.Rooms where r.RoomId == "M-01" select r).FirstOrDefault();
    }

    // จำนวนวันที่ให้ชำระหลังจอง ถ้าเลยแล้วยังไม่ชำระ การจองถูกยกเลิกอัตโนมัติ
    public const int PayWithinDays = 7;

    // ยกเลิกการจองที่ยังรอชำระ (Pending) เกินกำหนด: การจองเป็น Cancelled และห้องกลับมาว่าง
    // ไม่มีงานตั้งเวลาในระบบ จึงเรียกตอนเปิดหน้าที่เกี่ยวข้อง (หน้าลูกค้า และหน้าภาพรวมฝ่ายบัญชี)
    public static int CancelExpiredBookings(Csi402dbContext db)
    {
        var limit = DateTime.Now.AddDays(-PayWithinDays);
        var expired = (from b in db.Bookings
                       where b.BookingStatus == "Pending" && b.BookingDate < limit
                       select b).ToList();

        foreach (var booking in expired)
        {
            CancelBooking(db, booking, "[ยกเลิกอัตโนมัติ " + DateTime.Now.ToString("d MMM yyyy", _th) + "] ไม่ชำระภายใน " + PayWithinDays + " วัน");
        }
        if (expired.Count > 0)
        {
            db.SaveChanges();
        }
        return expired.Count;
    }

    // ยกเลิกการจอง 1 รายการ (ยังไม่ SaveChanges): สถานะ Cancelled, จดเหตุผลใน Note, ห้องกลับมาว่าง
    private static void CancelBooking(Csi402dbContext db, Booking booking, string noteLine)
    {
        booking.BookingStatus = "Cancelled";
        booking.Note = string.IsNullOrEmpty(booking.Note) ? noteLine : booking.Note + "\n" + noteLine;

        var room = (from r in db.Rooms where r.RoomId == booking.RoomId select r).FirstOrDefault();
        if (room != null && room.Status == "Booked")
        {
            room.Status = "Available";
        }
    }

    // จำนวนเดือนของเงินประกัน: สัญญา 1–3 เดือน = 1 เดือน, 6 เดือน = 2 เดือน, 12 เดือน = 3 เดือน
    public static int GetDepositMonths(int months)
    {
        if (months >= 12) return 3;
        if (months >= 6) return 2;
        return 1;
    }

    // วันสิ้นสุดสัญญา = วันก่อนวันที่เดียวกันของเดือนปลายทาง (เริ่ม 1 ธ.ค. สัญญา 6 เดือน = 31 พ.ค.)
    // ถ้าเดือนปลายทางไม่มีวันที่นั้น (เริ่ม 31 ม.ค. สัญญา 1 เดือน) ให้จบวันสุดท้ายของเดือนนั้น (28 ก.พ.)
    private static DateTime GetEndDate(DateTime start, int months)
    {
        var endMonth = new DateTime(start.Year, start.Month, 1).AddMonths(months);
        var lastDay = DateTime.DaysInMonth(endMonth.Year, endMonth.Month);
        if (start.Day > lastDay)
        {
            return endMonth.AddDays(lastDay - 1);
        }
        return endMonth.AddDays(start.Day - 2);
    }

    // ระยะสัญญาที่เลือกได้ (ค่าอื่นใช้ 6 เดือน)
    private static int ValidMonths(int months) => months == 1 || months == 3 || months == 6 || months == 12 ? months : 6;

    // ยอดที่ต้องชำระก่อนเข้าใช้ (ค่าเช่าเดือนแรก + เงินประกัน ไม่มี VAT) และวันสิ้นสุดสัญญา
    // ที่เดียวที่คิดยอดสำหรับแสดงผล ใช้กฎเดียวกับตอนบันทึกการจอง (GetDepositMonths / GetEndDate)
    public static PriceQuoteViewModel BuildQuote(decimal monthlyRate, DateTime start, int months)
    {
        months = ValidMonths(months);
        var depositMonths = GetDepositMonths(months);
        var deposit = monthlyRate * depositMonths;

        PriceQuoteViewModel q = new();
        q.Months = months;
        q.RentText = "฿" + monthlyRate.ToString("N0");
        q.DepositMonths = depositMonths;
        q.DepositText = "฿" + deposit.ToString("N0");
        q.TotalText = "฿" + (monthlyRate + deposit).ToString("N0");
        q.NextMonthsText = months > 1 ? "เดือนที่ 2–" + months + " ชำระเดือนละ ฿" + monthlyRate.ToString("N0") : null;
        q.EndDateText = GetEndDate(start.Date, months).ToString("d MMMM yyyy", _th);
        return q;
    }

    // เลขการจองใหม่ เช่น BK-2026-001
    private string NewBookingId()
    {
        var prefix = "BK-" + DateTime.Now.Year + "-";
        int no = (from b in _db.Bookings where b.BookingId.StartsWith(prefix) select b).Count() + 1;
        while ((from b in _db.Bookings where b.BookingId == prefix + no.ToString("000") select b).Any())
        {
            no++;
        }
        return prefix + no.ToString("000");
    }

    // การจองของผู้ใช้ที่ล็อกอิน ใหม่สุดขึ้นก่อน
    private List<MyBookingViewModel> GetMyBookings(User? user)
    {
        if (user == null)
        {
            return new List<MyBookingViewModel>();
        }

        var rows = (from b in _db.Bookings
                    join r in _db.Rooms on b.RoomId equals r.RoomId
                    where b.UserId == user.UserId
                    orderby b.BookingDate descending
                    select new { Booking = b, Room = r }).ToList();

        List<MyBookingViewModel> list = new();
        foreach (var row in rows)
        {
            int rate = (int)row.Room.MonthlyRate;
            MyBookingViewModel item = new();
            item.Id = row.Booking.BookingId;
            item.Room = row.Room.RoomId;
            item.Zone = row.Room.Zone ?? "";
            item.Period = row.Booking.StartDate.ToString("d MMM yyyy", _th) + " – " + row.Booking.EndDate.ToString("d MMM yyyy", _th);
            item.Months = rate > 0 ? (int)(row.Booking.TotalRent / rate) : 0;
            item.Rate = rate;
            item.Status = row.Booking.BookingStatus.ToLower();
            item.BookedDate = row.Booking.BookingDate;
            item.BookedOn = row.Booking.BookingDate.ToString("d MMM yyyy", _th);
            list.Add(item);
        }
        return list;
    }

    // ===================== หน้า =====================

    public IActionResult Dashboard()
    {
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        SetUserBag();
        CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล
        return View();
    }

    public IActionResult Search()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        SetUserBag();
        CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล

        var rooms = (from r in _db.Rooms select r).ToList();
        List<RoomViewModel> model = new();
        foreach (var r in rooms)
        {
            model.Add(ToRoomViewModel(r, GetRating(r.RoomId)));
        }
        return View(model);
    }

    // ยอดและวันสิ้นสุดสัญญา (JSON) หน้า RoomDetail / BookRoom ขอใหม่เมื่อลูกค้าเปลี่ยนวันเริ่มหรือระยะสัญญา
    [HttpGet]
    public IActionResult PriceQuote(string id, DateTime? start, int months = 6)
    {
        var room = FindRoom(id);
        if (room == null)
        {
            return NotFound();
        }
        var startDate = start != null && start.Value.Date >= DateTime.Today ? start.Value.Date : DateTime.Today;
        return Json(BuildQuote(room.MonthlyRate, startDate, months));
    }

    public IActionResult RoomDetail(string id = "M-01")
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        SetUserBag();
        CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล

        var room = FindRoom(id);
        if (room == null)
        {
            return RedirectToAction("Search");
        }

        // รีวิวของห้องนี้ (รีวิวผูกกับการจอง จึงต้อง join booking และ user)
        var reviews = (from rv in _db.Reviews
                       join b in _db.Bookings on rv.BookingId equals b.BookingId
                       join u in _db.Users on b.UserId equals u.UserId
                       where b.RoomId == room.RoomId
                       orderby rv.ReviewDate descending
                       select new { rv, u }).ToList();

        List<ReviewViewModel> reviewList = new();
        foreach (var row in reviews)
        {
            ReviewViewModel item = new();
            item.RoomId = room.RoomId;
            item.Name = string.IsNullOrEmpty(row.u.CompanyName) ? row.u.Fullname : row.u.CompanyName;
            item.Stars = row.rv.OverallRating;
            item.Date = row.rv.ReviewDate.ToString("MMM yyyy", _th);
            item.Comment = row.rv.Comment ?? "";
            reviewList.Add(item);
        }
        ViewBag.Reviews = reviewList;

        // เขียนรีวิวได้เมื่อเคยเช่าห้องนี้ (สัญญาใช้งานอยู่หรือสิ้นสุดแล้ว) และการจองนั้นยังไม่เคยรีวิว
        var user = GetCurrentUser();
        ViewBag.CanReview = user != null && (from b in _db.Bookings
                                              where b.UserId == user.UserId && b.RoomId == room.RoomId
                                                    && (b.BookingStatus == "Active" || b.BookingStatus == "Ended")
                                                    && !_db.Reviews.Any(rv => rv.BookingId == b.BookingId)
                                              select b).Any();

        ViewBag.Quote = BuildQuote(room.MonthlyRate, DateTime.Today, 6);
        var rating = reviewList.Count == 0 ? null : reviewList.Average(r => r.Stars).ToString("0.0");
        return View(ToRoomViewModel(room, rating));
    }

    [HttpPost]
    public IActionResult AddReview(ReviewViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }

        var room = FindRoom(data.RoomId);
        if (room == null)
        {
            return RedirectToAction("Search");
        }

        if (string.IsNullOrWhiteSpace(data.Comment))
        {
            TempData["Error"] = "กรุณาพิมพ์ความคิดเห็นก่อนส่งรีวิว";
            return RedirectToAction("RoomDetail", new { id = room.RoomId });
        }

        // หาการจองห้องนี้ของผู้ใช้ที่ยังไม่เคยรีวิว
        var user = GetCurrentUser();
        var booking = user == null ? null :
                      (from b in _db.Bookings
                       where b.UserId == user.UserId && b.RoomId == room.RoomId
                             && (b.BookingStatus == "Active" || b.BookingStatus == "Ended")
                             && !_db.Reviews.Any(rv => rv.BookingId == b.BookingId)
                       orderby b.BookingDate descending
                       select b).FirstOrDefault();
        if (booking == null)
        {
            TempData["Error"] = "รีวิวได้เฉพาะห้องที่เคยเช่า และรีวิวได้ 1 ครั้งต่อการจอง";
            return RedirectToAction("RoomDetail", new { id = room.RoomId });
        }

        // บันทึกลงตาราง review
        Review review = new();
        review.BookingId = booking.BookingId;
        review.OverallRating = Math.Clamp(data.Stars, 1, 5);
        review.Comment = data.Comment.Trim();
        review.ReviewDate = DateTime.Now;
        _db.Reviews.Add(review);
        _db.SaveChanges();

        TempData["Success"] = "ส่งรีวิวแล้ว ขอบคุณสำหรับความคิดเห็น";
        return RedirectToAction("RoomDetail", new { id = room.RoomId });
    }

    [HttpGet]
    public IActionResult BookRoom(string id = "M-01", DateTime? startDate = null, int months = 6)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        SetUserBag();
        CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล

        var room = FindRoom(id);
        if (room == null)
        {
            return RedirectToAction("Search");
        }
        ViewBag.Room = ToRoomViewModel(room, null);

        // ค่าเริ่มต้นของฟอร์ม: ข้อมูลผู้เช่าจากตาราง user, วันเริ่มเช่าและระยะสัญญาอาจส่งมาจากหน้า RoomDetail
        var user = GetCurrentUser();
        BookingViewModel data = new();
        data.RoomId = room.RoomId;
        data.StartDate = startDate != null && startDate.Value.Date >= DateTime.Today ? startDate.Value.Date : DateTime.Today;
        data.Months = ValidMonths(months);
        ViewBag.Quote = BuildQuote(room.MonthlyRate, data.StartDate, data.Months);
        data.ContactName = user?.Fullname ?? HttpContext.Session.GetString("FullName");
        data.CompanyName = user?.CompanyName;
        data.TaxId = FormatTaxId(user?.TaxId);
        data.Phone = FormatPhone(user?.Phone);
        data.Email = user?.Email;
        data.Address = user?.Address;
        return View(data);
    }

    [HttpPost]
    public IActionResult BookRoom(BookingViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        SetUserBag();
        CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล

        var room = FindRoom(data.RoomId);
        if (room == null)
        {
            return RedirectToAction("Search");
        }
        ViewBag.Room = ToRoomViewModel(room, null);
        // ยอดที่แสดงถ้าต้องกลับไปหน้าฟอร์ม (กรอกผิด) ใช้วันและระยะสัญญาที่ลูกค้าเลือกไว้
        ViewBag.Quote = BuildQuote(room.MonthlyRate, data.StartDate.Date >= DateTime.Today ? data.StartDate : DateTime.Today, data.Months);

        var user = GetCurrentUser();
        if (user == null)
        {
            ViewBag.Error = "บัญชีนี้จองห้องไม่ได้ กรุณาเข้าสู่ระบบด้วยบัญชีลูกค้าที่สมัครสมาชิก";
            return View(data);
        }

        if (room.Status != "Available")
        {
            ViewBag.Error = "ห้องนี้มีผู้เช่าแล้ว จองไม่ได้";
            return View(data);
        }

        data.Phone = DigitsOnly(data.Phone);
        data.TaxId = DigitsOnly(data.TaxId);
        bool isCompany = data.TenantType != "person";
        if ((isCompany && string.IsNullOrWhiteSpace(data.CompanyName))
            || string.IsNullOrWhiteSpace(data.ContactName)
            || string.IsNullOrWhiteSpace(data.Phone)
            || string.IsNullOrWhiteSpace(data.Email)
            || string.IsNullOrWhiteSpace(data.Address))
        {
            ViewBag.Error = "กรอกข้อมูลผู้เช่าให้ครบทุกช่อง";
            return View(data);
        }

        if (data.TaxId == null || data.TaxId.Length != 13 || !data.TaxId.All(char.IsDigit))
        {
            ViewBag.Error = isCompany ? "เลขผู้เสียภาษีต้องเป็นตัวเลข 13 หลัก" : "เลขบัตรประชาชนต้องเป็นตัวเลข 13 หลัก";
            return View(data);
        }

        if (data.StartDate.Date < DateTime.Today)
        {
            ViewBag.Error = "เลือกวันเริ่มเช่าย้อนหลังไม่ได้";
            return View(data);
        }

        if (data.Months != 1 && data.Months != 3 && data.Months != 6 && data.Months != 12)
        {
            ViewBag.Error = "ระยะสัญญาไม่ถูกต้อง";
            return View(data);
        }

        // สินค้าที่จะนำเข้า: ตัดแถวที่ว่างทั้งแถวออก ต้องเหลืออย่างน้อย 1 รายการ
        var items = (from i in data.Items
                     where !string.IsNullOrWhiteSpace(i.ItemName) || i.Quantity != 0 || !string.IsNullOrWhiteSpace(i.Unit)
                     select i).ToList();
        data.Items = items;
        if (items.Count == 0)
        {
            ViewBag.Error = "กรุณาแจ้งสินค้าที่จะนำเข้าอย่างน้อย 1 รายการ";
            return View(data);
        }
        if (items.Any(i => string.IsNullOrWhiteSpace(i.ItemName)))
        {
            ViewBag.Error = "กรุณากรอกชื่อสินค้าให้ครบทุกรายการ";
            return View(data);
        }
        if (items.Any(i => i.Quantity <= 0))
        {
            ViewBag.Error = "จำนวนสินค้าต้องมากกว่า 0 ทุกรายการ";
            return View(data);
        }

        if (!data.AgreeTerms)
        {
            ViewBag.Error = "กรุณายอมรับเงื่อนไขสัญญาเช่าก่อนยืนยัน";
            return View(data);
        }

        // ข้อมูลผู้เช่าบันทึกลงตาราง user (ใช้ออกใบกำกับภาษี)
        user.CompanyName = isCompany ? data.CompanyName!.Trim() : null;
        user.TaxId = data.TaxId;
        user.Phone = data.Phone!.Trim();
        user.Email = data.Email!.Trim();
        user.Address = data.Address!.Trim();

        // หมายเหตุของการจอง: ผู้ติดต่อ + บริการวันย้ายเข้า + หมายเหตุที่ลูกค้าพิมพ์
        var note = "ผู้ติดต่อ: " + data.ContactName!.Trim();
        if (!string.IsNullOrEmpty(data.MoveInService) && data.MoveInService != "ไม่ต้องการ")
        {
            note += "\nบริการวันย้ายเข้า: " + data.MoveInService;
        }
        if (!string.IsNullOrWhiteSpace(data.Note))
        {
            note += "\n" + data.Note.Trim();
        }

        // บันทึกลงตาราง booking
        var start = data.StartDate.Date;
        var end = GetEndDate(start, data.Months);

        Booking booking = new();
        booking.BookingId = NewBookingId();
        booking.UserId = user.UserId;
        booking.RoomId = room.RoomId;
        booking.BookingDate = DateTime.Now;
        booking.StartDate = DateOnly.FromDateTime(start);
        booking.EndDate = DateOnly.FromDateTime(end);
        booking.TotalRent = room.MonthlyRate * data.Months;
        booking.Deposit = room.MonthlyRate * GetDepositMonths(data.Months);
        booking.PromoCode = string.IsNullOrWhiteSpace(data.PromoCode) ? null : data.PromoCode.Trim();
        booking.GoodsCategory = data.GoodsType;
        booking.Note = note;
        booking.BookingStatus = "Pending";
        _db.Bookings.Add(booking);

        // สินค้าที่ลูกค้าแจ้ง บันทึกลงตาราง stockitem
        // UpdatedBy = ลูกค้า หมายถึง "รอตรวจรับ" (ฝ่ายคลังตรวจรับแล้ว UpdatedBy จะเป็นพนักงาน)
        foreach (var item in items)
        {
            Stockitem stock = new();
            stock.BookingId = booking.BookingId;
            stock.ItemName = item.ItemName!.Trim();
            stock.Category = data.GoodsType;
            stock.Quantity = item.Quantity;
            stock.MinQuantity = 0;   // ฝ่ายคลังเป็นคนตั้งตอนตรวจรับ
            stock.Unit = string.IsNullOrWhiteSpace(item.Unit) ? null : item.Unit.Trim();
            stock.LastUpdated = DateTime.Now;
            stock.UpdatedBy = user.UserId;
            _db.Stockitems.Add(stock);
        }

        // ห้องถูกจองแล้ว คนอื่นจองซ้ำไม่ได้
        room.Status = "Booked";
        _db.SaveChanges();

        TempData["Success"] = "จองห้อง " + room.RoomId + " แล้ว (" + booking.BookingId + ") กรุณาชำระเงินเพื่อยืนยันการจอง";
        return RedirectToAction("Profile", new { tab = "payment", bookingId = booking.BookingId });
    }

    public IActionResult Profile(string tab = "bookings", string? bookingId = null)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        SetUserBag();
        CancelExpiredBookings(_db);   // ยกเลิกการจองที่ไม่ชำระเกินกำหนดก่อนแสดงผล

        if (tab != "bookings" && tab != "payment" && tab != "stock" && tab != "info")
        {
            tab = "bookings";
        }
        ViewBag.ActiveTab = tab;

        var user = GetCurrentUser();
        var bookings = GetMyBookings(user);
        ViewBag.Bookings = bookings;

        // สต็อกสินค้า (ดูอย่างเดียว ฝ่ายคลังเป็นคนปรับ) ของการจองที่ยังไม่สิ้นสุด
        // รวมการจองที่ยังรอชำระ ให้ลูกค้าเห็นรายการที่แจ้งไว้ (สถานะรอตรวจรับ)
        var activeIds = (from b in bookings
                         where b.Status == "active" || b.Status == "pending" || b.Status == "checking"
                         select b.Id).ToList();
        ViewBag.Stock = user == null
            ? new List<StockItemViewModel>()
            : (from s in InventoryController.GetStockItems(_db, null, false, user.UserId)
               where activeIds.Contains(s.BookingId)
               select s).ToList();

        // ประวัติการชำระจากตาราง payment
        List<PaymentHistoryViewModel> payments = new();
        if (user != null)
        {
            var rows = (from p in _db.Payments
                        join b in _db.Bookings on p.BookingId equals b.BookingId
                        where b.UserId == user.UserId
                        orderby p.PaymentDate descending
                        select new
                        {
                            p,
                            b,
                            ReceiptId = (from r in _db.Receipts where r.PaymentId == p.PaymentId select r.ReceiptId).FirstOrDefault()
                        }).ToList();
            foreach (var row in rows)
            {
                PaymentHistoryViewModel item = new();
                item.No = "PAY-" + row.p.PaymentId.ToString("00000");
                item.Title = row.b.RoomId + " ค่าเช่าเดือนแรกและเงินประกัน";
                item.Amount = (int)row.p.Amount;
                item.Detail = (row.p.PaymentDate?.ToString("d MMM yyyy", _th) ?? "-") + ", "
                              + (_payMethodText.ContainsKey(row.p.PaymentMethod) ? _payMethodText[row.p.PaymentMethod] : row.p.PaymentMethod)
                              + (row.p.PaymentStatus == "Pending" ? " (รอตรวจสอบ)"
                                 : row.p.PaymentStatus == "Approved" ? " (อนุมัติแล้ว)"
                                 : row.p.PaymentStatus == "Rejected" ? " (ถูกปฏิเสธ)" : "");
                item.ReceiptId = row.ReceiptId;
                item.Date = row.p.PaymentDate ?? DateTime.MinValue;
                payments.Add(item);
            }

            // การคืนเงินจากตาราง refund (แสดงเป็นยอดติดลบ)
            var refundRows = (from rf in _db.Refunds
                              join p in _db.Payments on rf.PaymentId equals p.PaymentId
                              join b in _db.Bookings on p.BookingId equals b.BookingId
                              where b.UserId == user.UserId && rf.RefundStatus == "Completed"
                              select new { rf, b }).ToList();
            foreach (var row in refundRows)
            {
                PaymentHistoryViewModel item = new();
                item.No = row.rf.RefundId;
                item.Title = "คืนเงินประกัน " + row.b.RoomId;
                item.Amount = (int)row.rf.RefundAmount;
                item.Detail = (row.rf.RefundDate?.ToString("d MMM yyyy", _th) ?? "-") + ", " + (row.rf.RefundMethod ?? "-");
                item.IsRefund = true;
                item.Date = row.rf.RefundDate ?? DateTime.MinValue;
                payments.Add(item);
            }
            payments = payments.OrderByDescending(x => x.Date).ToList();
        }
        ViewBag.Payments = payments;

        // การจองที่ต้องชำระ: ตามเลขที่ส่งมา ถ้าไม่มีใช้รายการรอชำระล่าสุด
        var due = (from b in bookings where b.Id == bookingId && b.Status == "pending" select b).FirstOrDefault()
                  ?? (from b in bookings where b.Status == "pending" select b).FirstOrDefault();
        ViewBag.DueBooking = due;
        if (due != null)
        {
            var depositMonths = GetDepositMonths(due.Months);
            ViewBag.DueDepositMonths = depositMonths;
            ViewBag.DueDeposit = due.Rate * depositMonths;
            ViewBag.DueTotal = due.Rate + due.Rate * depositMonths;
            ViewBag.DueDate = due.BookedDate.AddDays(7).ToString("d MMM yyyy", _th);

            // ถ้าฝ่ายบัญชีเคยปฏิเสธการชำระของการจองนี้ ให้แสดงเหตุผลล่าสุด (เก็บไว้ใน booking.Note)
            var note = (from b in _db.Bookings where b.BookingId == due.Id select b.Note).FirstOrDefault() ?? "";
            var rejectLine = note.Split('\n').LastOrDefault(l => l.StartsWith(AccountingController.RejectNotePrefix));
            if (rejectLine != null)
            {
                ViewBag.RejectReason = rejectLine.Substring(rejectLine.IndexOf(']') + 1).Trim();
            }
        }

        // ข้อมูลส่วนตัวจากตาราง user
        ProfileViewModel info = new();
        info.Fullname = user?.Fullname ?? HttpContext.Session.GetString("FullName") ?? "";
        info.Company = user?.CompanyName ?? "";
        info.Phone = FormatPhone(user?.Phone) ?? "";
        info.Email = user?.Email ?? "";
        info.TaxId = FormatTaxId(user?.TaxId) ?? "";
        info.Address = user?.Address ?? "";
        return View(info);
    }

    [HttpPost]
    public IActionResult Pay(PaymentViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }

        var user = GetCurrentUser();
        var booking = user == null ? null :
                      (from b in _db.Bookings
                       where b.BookingId == data.BookingId && b.UserId == user.UserId
                       select b).FirstOrDefault();
        if (booking == null || booking.BookingStatus != "Pending")
        {
            TempData["Error"] = "ไม่พบรายการที่ต้องชำระ";
            return RedirectToAction("Profile", new { tab = "payment" });
        }

        // รับชำระแค่โอนเงินกับ QR พร้อมเพย์ (ยังไม่มีระบบตัดบัตร)
        if (data.PayMethod != "transfer" && data.PayMethod != "qr")
        {
            TempData["Error"] = "วิธีชำระเงินไม่ถูกต้อง";
            return RedirectToAction("Profile", new { tab = "payment", bookingId = booking.BookingId });
        }

        // ทั้งโอนเงินและ QR ต้องแนบสลิปให้ฝ่ายบัญชีตรวจ เก็บไฟล์ไว้ที่ wwwroot/uploads/slips
        string? slipUrl = null;
        if (data.Slip == null || data.Slip.Length == 0)
        {
            TempData["Error"] = "กรุณาแนบสลิปการชำระ";
            return RedirectToAction("Profile", new { tab = "payment", bookingId = booking.BookingId });
        }

        var ext = Path.GetExtension(data.Slip.FileName).ToLower();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
        {
            TempData["Error"] = "สลิปต้องเป็นไฟล์รูป (jpg, png, webp)";
            return RedirectToAction("Profile", new { tab = "payment", bookingId = booking.BookingId });
        }

        var folder = Path.Combine(_env.WebRootPath, "uploads", "slips");
        Directory.CreateDirectory(folder);
        var fileName = booking.BookingId + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext;
        using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
        {
            data.Slip.CopyTo(stream);
        }
        slipUrl = "/uploads/slips/" + fileName;

        // บันทึกลงตาราง payment: ค่าเช่าเดือนแรก + เงินประกัน รอเจ้าหน้าที่ตรวจสอบ
        var room = (from r in _db.Rooms where r.RoomId == booking.RoomId select r).First();
        Payment payment = new();
        payment.BookingId = booking.BookingId;
        payment.Amount = room.MonthlyRate + booking.Deposit;
        payment.PaymentMethod = data.PayMethod;
        payment.PaymentDate = DateTime.Now;
        payment.PaymentStatus = "Pending";
        payment.SlipImageUrl = slipUrl;
        payment.DueDate = booking.BookingDate.AddDays(7);
        _db.Payments.Add(payment);

        booking.BookingStatus = "Checking";
        _db.SaveChanges();

        TempData["Success"] = "ได้รับแจ้งชำระเงินแล้ว เจ้าหน้าที่จะตรวจสอบและอัปเดตสถานะภายใน 15 นาที";
        return RedirectToAction("Profile", new { tab = "bookings" });
    }

    [HttpPost]
    public IActionResult SaveProfile(ProfileViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }

        var user = GetCurrentUser();
        if (user == null)
        {
            TempData["Error"] = "บัญชีนี้แก้ไขข้อมูลไม่ได้ กรุณาเข้าสู่ระบบด้วยบัญชีลูกค้าที่สมัครสมาชิก";
            return RedirectToAction("Profile", new { tab = "info" });
        }

        data.Phone = DigitsOnly(data.Phone) ?? "";
        data.TaxId = DigitsOnly(data.TaxId) ?? "";
        if (string.IsNullOrWhiteSpace(data.Fullname) || string.IsNullOrWhiteSpace(data.Phone) || string.IsNullOrWhiteSpace(data.Email))
        {
            TempData["Error"] = "กรุณากรอกชื่อ เบอร์โทร และอีเมล";
            return RedirectToAction("Profile", new { tab = "info" });
        }

        if (!string.IsNullOrWhiteSpace(data.TaxId) && (data.TaxId.Trim().Length != 13 || !data.TaxId.Trim().All(char.IsDigit)))
        {
            TempData["Error"] = "เลขผู้เสียภาษี / เลขบัตรประชาชนต้องเป็นตัวเลข 13 หลัก";
            return RedirectToAction("Profile", new { tab = "info" });
        }

        // บันทึกลงตาราง user (แบบเดียวกับ AdminController.EditUser)
        user.Fullname = data.Fullname.Trim();
        user.CompanyName = string.IsNullOrWhiteSpace(data.Company) ? null : data.Company.Trim();
        user.Phone = data.Phone.Trim();
        user.Email = data.Email.Trim();
        user.TaxId = string.IsNullOrWhiteSpace(data.TaxId) ? null : data.TaxId.Trim();
        user.Address = string.IsNullOrWhiteSpace(data.Address) ? null : data.Address.Trim();
        _db.SaveChanges();

        HttpContext.Session.SetString("FullName", user.Fullname);

        TempData["Success"] = "บันทึกข้อมูลแล้ว";
        return RedirectToAction("Profile", new { tab = "info" });
    }

    // ใบเสร็จสำหรับพิมพ์ (ลูกค้าเปิดได้เฉพาะของตัวเอง)
    public IActionResult Receipt(string id)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }

        var user = GetCurrentUser();
        var isMine = user != null && (from r in _db.Receipts
                                      join p in _db.Payments on r.PaymentId equals p.PaymentId
                                      join b in _db.Bookings on p.BookingId equals b.BookingId
                                      where r.ReceiptId == id && b.UserId == user.UserId
                                      select r).Any();
        var doc = isMine ? AccountingController.BuildReceiptDocument(_db, id) : null;
        if (doc == null)
        {
            TempData["Error"] = "ไม่พบใบเสร็จนี้";
            return RedirectToAction("Profile", new { tab = "payment" });
        }
        doc.BackUrl = Url.Action("Profile", "Customer", new { tab = "payment" })!;
        return View("ReceiptDocument", doc);
    }

    // ===================== แจ้งปัญหา (กล่องแชท CS มุมขวาล่าง) =====================
    // ข้างในกล่องแชทเป็นหน้าย่อย (iframe) ที่ใช้ _ChatLayout: Chat = รายการเรื่อง, ChatThread = คุยในเรื่อง, ChatNew = เรื่องใหม่
    // ส่งข้อความ / เปิดเรื่อง เป็นฟอร์ม POST มาที่ Controller ตามปกติ

    // จำนวนข้อความของเจ้าหน้าที่ที่ลูกค้าเปิดอ่านแล้ว (เก็บใน Session เพราะตารางไม่มีช่องสถานะอ่านแล้ว)
    private int GetSeenCount(int ticketId) => HttpContext.Session.GetInt32("TicketSeen:" + ticketId) ?? 0;

    private void MarkSeen(TicketViewModel ticket)
    {
        HttpContext.Session.SetInt32("TicketSeen:" + ticket.TicketId, ticket.Messages.Count(m => m.IsStaff));
    }

    // เรื่องทั้งหมดของลูกค้าที่ล็อกอิน พร้อมจำนวนข้อความใหม่จากเจ้าหน้าที่ในแต่ละเรื่อง
    private List<TicketViewModel> GetMyTickets()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return new List<TicketViewModel>();
        }
        var tickets = CustomerServiceController.GetTickets(_db, user.UserId, null, null, true);
        foreach (var t in tickets)
        {
            t.Unread = Math.Max(t.Messages.Count(m => m.IsStaff) - GetSeenCount(t.TicketId), 0);
        }
        // เรียงตามข้อความล่าสุด (เรื่องที่มีข้อความใหม่ขึ้นก่อน)
        return tickets.OrderByDescending(t => t.Unread > 0).ThenByDescending(t => t.TicketId).ToList();
    }

    // รายการเรื่องในกล่องแชท
    public IActionResult Chat()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        ViewBag.CanChat = GetCurrentUser() != null;
        return View(GetMyTickets());
    }

    // คุยในเรื่อง 1 เรื่อง (ในกล่องแชท): เปิดแล้วนับว่าอ่านแล้ว
    public IActionResult ChatThread(int id)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }

        var user = GetCurrentUser();
        var ticket = user == null ? null : CustomerServiceController.GetTickets(_db, user.UserId, null, id, true).FirstOrDefault();
        if (ticket == null)
        {
            TempData["Error"] = "ไม่พบเรื่องนี้";
            return RedirectToAction("Chat");
        }
        MarkSeen(ticket);
        return View(ticket);
    }

    // ฟอร์มเปิดเรื่องใหม่ (ในกล่องแชท)
    [HttpGet]
    public IActionResult ChatNew()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        var user = GetCurrentUser();
        if (user == null)
        {
            TempData["Error"] = "บัญชีนี้แจ้งปัญหาไม่ได้ กรุณาเข้าสู่ระบบด้วยบัญชีลูกค้าที่สมัครสมาชิก";
            return RedirectToAction("Chat");
        }
        ViewBag.TicketBookings = (from b in GetMyBookings(user) where b.Status != "cancelled" select b).ToList();
        return View(new NewTicketViewModel());
    }

    // เปิดเรื่องใหม่ (POST): ลูกค้าเลือกหมวด ระบบตั้งความด่วนให้ตามหมวด
    [HttpPost]
    public IActionResult ChatNew(NewTicketViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }

        var user = GetCurrentUser();
        if (user == null)
        {
            TempData["Error"] = "บัญชีนี้แจ้งปัญหาไม่ได้ กรุณาเข้าสู่ระบบด้วยบัญชีลูกค้าที่สมัครสมาชิก";
            return RedirectToAction("Chat");
        }
        ViewBag.TicketBookings = (from b in GetMyBookings(user) where b.Status != "cancelled" select b).ToList();

        if (data.Category == null || !CustomerServiceController.Categories.ContainsKey(data.Category))
        {
            ViewBag.Error = "กรุณาเลือกหมวดของปัญหา";
            return View(data);
        }
        if (string.IsNullOrWhiteSpace(data.Subject) || string.IsNullOrWhiteSpace(data.Description))
        {
            ViewBag.Error = "กรุณากรอกหัวข้อและรายละเอียด";
            return View(data);
        }

        // การจองที่เลือก ต้องเป็นของลูกค้าคนนี้
        string? bookingId = null;
        if (!string.IsNullOrEmpty(data.BookingId))
        {
            bookingId = (from b in _db.Bookings
                         where b.BookingId == data.BookingId && b.UserId == user.UserId
                         select b.BookingId).FirstOrDefault();
        }

        // หมวดเก็บไว้หน้าหัวข้อ (ตารางไม่มีช่องหมวด) เช่น "[ไฟฟ้า] ไฟในห้องดับ"
        var subject = "[" + data.Category + "] " + data.Subject.Trim();
        if (subject.Length > 255)
        {
            subject = subject.Substring(0, 255);
        }

        Supportticket ticket = new();
        ticket.UserId = user.UserId;
        ticket.BookingId = bookingId;
        ticket.Subject = subject;
        ticket.Description = data.Description.Trim().Replace("\r\n", "\n");
        ticket.Priority = CustomerServiceController.Categories[data.Category];
        ticket.TicketStatus = "Open";
        ticket.CreatedAt = DateTime.Now;
        _db.Supporttickets.Add(ticket);
        _db.SaveChanges();

        TempData["Success"] = "ส่งเรื่องแล้ว ฝ่ายบริการลูกค้าจะตอบกลับในแชทนี้";
        return RedirectToAction("ChatThread", new { id = ticket.TicketId });
    }

    // จำนวนข้อความใหม่จากเจ้าหน้าที่ (JSON) ปุ่มแชทเรียกทุก 30 วินาทีเพื่อแสดงป้ายตัวเลขและแจ้งเตือน
    [HttpGet]
    public IActionResult ChatUnread()
    {
        if (!IsLoggedIn)
        {
            return Json(new { count = 0 });
        }
        if (IsStaff)
        {
            return Json(new { count = 0 });
        }
        var tickets = GetMyTickets();
        var latest = tickets.FirstOrDefault(t => t.Unread > 0);
        return Json(new
        {
            count = tickets.Sum(t => t.Unread),
            title = latest?.Title,
            text = latest?.Messages.LastOrDefault(m => m.IsStaff)?.Text
        });
    }

    // หน้าตั๋วแบบเต็มหน้า (เก็บไว้เผื่อเข้าจากลิงก์เดิม) เปิดได้เฉพาะตั๋วของตัวเอง
    public IActionResult Ticket(int id)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        SetUserBag();

        var user = GetCurrentUser();
        var ticket = user == null ? null : CustomerServiceController.GetTickets(_db, user.UserId, null, id, true).FirstOrDefault();
        if (ticket == null)
        {
            TempData["Error"] = "ไม่พบเรื่องนี้";
            return RedirectToAction("Profile");
        }
        MarkSeen(ticket);
        return View(ticket);
    }

    // ลูกค้าตอบในเรื่อง (POST): ถ้าเรื่องปิดไปแล้ว จะเปิดใหม่ให้พนักงานเห็น
    // Embed = true ส่งมาจากกล่องแชท กลับไปที่ ChatThread, ไม่งั้นกลับหน้าเต็ม Ticket
    [HttpPost]
    public IActionResult ReplyTicket(TicketReplyViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        var backAction = data.Embed ? "ChatThread" : "Ticket";

        var user = GetCurrentUser();
        var ticket = user == null ? null :
                     (from t in _db.Supporttickets
                      where t.TicketId == data.TicketId && t.UserId == user.UserId
                      select t).FirstOrDefault();
        if (ticket == null)
        {
            TempData["Error"] = "ไม่พบเรื่องนี้";
            return RedirectToAction(data.Embed ? "Chat" : "Profile");
        }
        if (string.IsNullOrWhiteSpace(data.Message))
        {
            TempData["Error"] = "กรุณาพิมพ์ข้อความก่อนส่ง";
            return RedirectToAction(backAction, new { id = data.TicketId });
        }

        ticket.Description = CustomerServiceController.AppendMessage(ticket.Description, user!.Fullname, false, data.Message);
        bool reopened = ticket.TicketStatus == "Resolved";
        if (reopened)
        {
            ticket.TicketStatus = "Open";
            ticket.ResolvedAt = null;
        }
        _db.SaveChanges();

        if (reopened)
        {
            TempData["Success"] = "เปิดเรื่องนี้อีกครั้งแล้ว";
        }
        return RedirectToAction(backAction, new { id = data.TicketId });
    }

    // ลูกค้ายกเลิกการจองเอง (POST): ได้เฉพาะการจองของตัวเองที่ยังไม่ได้ชำระ
    [HttpPost]
    public IActionResult CancelMyBooking(string bookingId)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }

        var user = GetCurrentUser();
        var booking = user == null ? null :
                      (from b in _db.Bookings
                       where b.BookingId == bookingId && b.UserId == user.UserId
                       select b).FirstOrDefault();
        if (booking == null || booking.BookingStatus != "Pending")
        {
            TempData["Error"] = "ยกเลิกได้เฉพาะการจองที่ยังไม่ได้ชำระเงิน";
            return RedirectToAction("Profile", new { tab = "bookings" });
        }

        CancelBooking(_db, booking, "[ลูกค้ายกเลิก " + DateTime.Now.ToString("d MMM yyyy", _th) + "]");
        _db.SaveChanges();

        TempData["Success"] = "ยกเลิกการจอง " + booking.BookingId + " แล้ว";
        return RedirectToAction("Profile", new { tab = "bookings" });
    }

    public IActionResult MyBookings()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        return RedirectToAction("Profile", new { tab = "bookings" });
    }

    public IActionResult Payment(string? bookingId = null)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        return RedirectToAction("Profile", new { tab = "payment", bookingId });
    }

    public IActionResult Reviews()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        if (IsStaff)
        {
            return RedirectToAction("Dashboard", StaffRole);   // พนักงานฝ่ายอื่นกลับไปหน้าฝ่ายตัวเอง
        }
        return RedirectToAction("RoomDetail", new { id = "M-01" });
    }
}
