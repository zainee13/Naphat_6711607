using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class CustomerController : Controller
{
    // ===================== ข้อมูลตัวอย่าง (ยังไม่ต่อฐานข้อมูล) =====================
    // ตอนต่อฐานข้อมูล ให้เปลี่ยนไปดึงจากตาราง rooms / bookings / payments / reviews แทน

    public static readonly List<RoomViewModel> _rooms = new()
    {
        new RoomViewModel {
            Id = "XL-01", Kind = "xl", Free = true, Grid = "1 / 1 / 6 / 6",
            Area = "1,200", Height = "12 ม.", Zone = "โซน D1 (เมกะ)",
            Vehicle = "คอนเทนเนอร์ทุกขนาด", Temp = "อุณหภูมิปกติ",
            FloorLoad = "5 ตัน/ตร.ม.", Dock = "Dock Leveler 4 ช่อง", Power = "หม้อแปลงแยก 250 kVA",
            Note = "คลังโล่งขนาดใหญ่พิเศษ",
            Desc = "คลังโล่งขนาดใหญ่พิเศษ เพดานสูง 12 ม. หลังคา Solar Roof",
            Price = 180000, OldPrice = 210000, Rating = "5.0",
            Img = "https://images.unsplash.com/photo-1494412685616-a5d310fbb07d"
        },
        new RoomViewModel {
            Id = "L-01", Kind = "l", Free = true, Grid = "1 / 6 / 4 / 10",
            Area = "500", Height = "10 ม.", Zone = "โซน C1 (มี Dock)",
            Vehicle = "รถพ่วง 18 ล้อ / ตู้ 40ft", Temp = "อุณหภูมิปกติ",
            FloorLoad = "5 ตัน/ตร.ม.", Dock = "Dock Leveler 2 ช่อง", Power = "3 เฟส 380V (200A)",
            Note = "มี Dock Leveler เทียบตู้คอนเทนเนอร์",
            Desc = "รถพ่วง 18 ล้อเข้าได้ มี Dock Leveler ปรับระดับอัตโนมัติ 2 ช่องสำหรับเทียบตู้คอนเทนเนอร์",
            Price = 75000, OldPrice = 85000, Rating = "4.6",
            Img = "https://images.unsplash.com/photo-1553413077-190dd305871c"
        },
        new RoomViewModel {
            Id = "L-02", Kind = "l", Free = false, Grid = "1 / 10 / 4 / 13",
            Area = "350", Height = "10 ม.", Zone = "โซน C2 (สินค้าปลอดภัย)",
            Vehicle = "10 ล้อ / เทรลเลอร์", Temp = "อุณหภูมิปกติ",
            FloorLoad = "4 ตัน/ตร.ม.", Dock = "Dock Leveler 1 ช่อง", Power = "3 เฟส 380V (150A)",
            Note = "ควบคุมการเข้าออกเฉพาะบุคคล",
            Desc = "สำหรับสินค้ามูลค่าสูง ประตูนิรภัย 2 ชั้น กล้องบันทึกภาพย้อนหลัง 90 วัน",
            Price = 55000, OldPrice = 0, Rating = "4.5",
            Img = "https://images.unsplash.com/photo-1494412685616-a5d310fbb07d"
        },
        new RoomViewModel {
            Id = "M-02", Kind = "m", Free = true, Grid = "4 / 6 / 7 / 9",
            Area = "250", Height = "8 ม.", Zone = "โซน B2 (พาเลท)",
            Vehicle = "10 ล้อ", Temp = "อุณหภูมิปกติ",
            FloorLoad = "3.5 ตัน/ตร.ม.", Dock = "ชานชาลาสูง 1.2 ม.", Power = "3 เฟส 380V (100A)",
            Note = "Racking 3 ชั้น วางได้ 120 พาเลท",
            Desc = "ติดตั้ง Racking ได้ 3 ชั้น วางพาเลทได้ 120 ตัว มีระบบระบายอากาศ 24 ชม.",
            Price = 38000, OldPrice = 0, Rating = "4.8",
            Img = "https://images.unsplash.com/photo-1553413077-190dd305871c"
        },
        new RoomViewModel {
            Id = "M-01", Kind = "m", Free = true, Grid = "4 / 9 / 7 / 11",
            Area = "150", Height = "8 ม.", Zone = "โซน B1 (มาตรฐาน)",
            Vehicle = "6 ล้อ / 10 ล้อ", Temp = "อุณหภูมิปกติ",
            FloorLoad = "3 ตัน/ตร.ม.", Dock = "ชานชาลาพร้อมทางลาด", Power = "3 เฟส 380V (50A)",
            Note = "พื้นอีพ็อกซี่รับน้ำหนัก 3 ตัน/ตร.ม.",
            Desc = "พื้นอีพ็อกซี่รับน้ำหนัก 3 ตัน/ตร.ม. โฟล์คลิฟต์เข้าได้ วางพาเลทได้ 60–120 ตัว",
            Price = 25000, OldPrice = 29000, Rating = "4.8",
            Img = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d"
        },
        new RoomViewModel {
            Id = "CR-05", Kind = "cold", Free = true, Grid = "4 / 11 / 7 / 13",
            Area = "200", Height = "7 ม.", Zone = "โซนห้องเย็น",
            Vehicle = "10 ล้อตู้เย็น", Temp = "−18°C ถึง +5°C",
            FloorLoad = "3 ตัน/ตร.ม.", Dock = "Air Lock Loading Bay", Power = "3 เฟส + ไฟสำรอง",
            Note = "ควบคุม −18°C ถึง +5°C ดูอุณหภูมิผ่านมือถือ",
            Desc = "ห้องเย็นฉนวน PIR หนา 100 มม. ไฟสำรองคู่ แจ้งเตือนเมื่ออุณหภูมิผิดปกติทาง SMS",
            Price = 40000, OldPrice = 48000, Rating = "4.9",
            Img = "https://images.unsplash.com/photo-1553413077-190dd305871c"
        },
        new RoomViewModel {
            Id = "S-01", Kind = "s", Free = true, Grid = "6 / 1 / 7 / 3",
            Area = "60", Height = "6 ม.", Zone = "โซน A1 (ชั้น 1)",
            Vehicle = "กระบะ / 4 ล้อ", Temp = "อุณหภูมิปกติ",
            FloorLoad = "1.5 ตัน/ตร.ม.", Dock = "ทางลาดเทียบรถกระบะ", Power = "220V 1 เฟส",
            Note = "ชั้น 1 เข้าถึงง่าย",
            Desc = "ห้องไซส์เล็กชั้น 1 ประตูม้วนกว้าง 3.5 ม. เหมาะกับเอกสารและสต็อกสินค้าพร้อมส่ง",
            Price = 12000, OldPrice = 0, Rating = "4.7",
            Img = "https://images.unsplash.com/photo-1504307651254-35680f356dfd"
        },
        new RoomViewModel {
            Id = "S-02", Kind = "s", Free = true, Grid = "6 / 3 / 7 / 6",
            Area = "80", Height = "6 ม.", Zone = "โซน A2 (SME)",
            Vehicle = "4 ล้อ / 6 ล้อ", Temp = "อุณหภูมิปกติ",
            FloorLoad = "2 ตัน/ตร.ม.", Dock = "ทางลาดโหลดสินค้าในร่ม", Power = "220V / 380V",
            Note = "ประตูม้วนไฟฟ้า",
            Desc = "ห้องกะทัดรัด ประตูม้วนไฟฟ้า เหมาะกับธุรกิจ SME และร้านค้าออนไลน์",
            Price = 15000, OldPrice = 18000, Rating = "4.3",
            Img = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d"
        },
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

    // การจอง: Username = "" คือข้อมูลตัวอย่าง แสดงให้ทุกคน
    public static readonly List<MyBookingViewModel> _bookings = new()
    {
        new MyBookingViewModel { Id = "BK-2026-002", Username = "", Room = "L-01",  Zone = "โซน C1",     Period = "1 ธ.ค. 2569 – 31 พ.ค. 2570", Months = 6, Rate = 75000, Status = "pending",   BookedOn = "9 พ.ย. 2569" },
        new MyBookingViewModel { Id = "BK-2026-001", Username = "", Room = "M-01",  Zone = "โซน B1",     Period = "1 ก.ค. – 31 ธ.ค. 2569",      Months = 6, Rate = 25000, Status = "active",    BookedOn = "15 มิ.ย. 2569" },
        new MyBookingViewModel { Id = "BK-2025-018", Username = "", Room = "CR-05", Zone = "โซนห้องเย็น", Period = "1 ก.ค. – 31 ธ.ค. 2568",      Months = 6, Rate = 40000, Status = "ended",     BookedOn = "20 มิ.ย. 2568" },
        new MyBookingViewModel { Id = "BK-2025-009", Username = "", Room = "M-01",  Zone = "โซน B1",     Period = "1 ม.ค. – 30 มิ.ย. 2568",     Months = 6, Rate = 25000, Status = "cancelled", BookedOn = "10 ธ.ค. 2567" },
    };

    public static readonly List<PaymentHistoryViewModel> _payments = new()
    {
        new PaymentHistoryViewModel { No = "INV-2026-011", Title = "M-01 ค่าเช่า พ.ย. 2569", Amount = 25000, Detail = "1 พ.ย. 2569, โอนผ่าน SCB" },
        new PaymentHistoryViewModel { No = "INV-2026-010", Title = "M-01 ค่าเช่า ต.ค. 2569", Amount = 25000, Detail = "1 ต.ค. 2569, โอนผ่าน SCB" },
        new PaymentHistoryViewModel { No = "INV-2026-005", Title = "M-01 ค่าเช่าเดือนแรกและเงินประกัน", Amount = 75000, Detail = "15 มิ.ย. 2569, โอนผ่าน SCB" },
        new PaymentHistoryViewModel { No = "REF-2026-001", Title = "คืนเงินประกัน CR-05", Amount = 80000, Detail = "15 ม.ค. 2569, โอนคืนบัญชีเดิม", IsRefund = true },
    };

    // รีวิว: RoomId = "" คือรีวิวตัวอย่าง แสดงทุกห้อง
    public static readonly List<ReviewViewModel> _reviews = new()
    {
        new ReviewViewModel { RoomId = "", Name = "บริษัท สยาม โลจิสติกส์ โซลูชั่นส์", Stars = 5, Date = "ม.ค. 2569", Comment = "ห้องสะอาด พื้นเรียบ โฟล์คลิฟต์วิ่งคล่อง เทียบตู้คอนเทนเนอร์สะดวก" },
        new ReviewViewModel { RoomId = "", Name = "บจก. นำเจริญ ซัพพลาย", Stars = 5, Date = "ธ.ค. 2568", Comment = "เก็บพาเลทสินค้านำเข้าได้พอดี เจ้าหน้าที่ช่วยเรื่องเอกสารเร็ว" },
        new ReviewViewModel { RoomId = "", Name = "SuperShop Thailand", Stars = 5, Date = "พ.ย. 2568", Comment = "ทำสัญญาออนไลน์ง่าย ประตูม้วนไฟฟ้าใช้สะดวก รถเข้าส่งของได้ง่าย" },
    };

    private static readonly CultureInfo _th = new("th-TH");

    // ===================== ตัวช่วย =====================

    private bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    private void SetUserBag()
    {
        var username = HttpContext.Session.GetString("Username");
        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(username);
        ViewBag.Username = username;
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้เยี่ยมชม";
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "Guest";
    }

    // หาห้องตามรหัส (รองรับรหัสเก่า) ถ้าไม่เจอให้ใช้ M-01
    private RoomViewModel FindRoom(string id)
    {
        if (_oldRoomIds.ContainsKey(id))
        {
            id = _oldRoomIds[id];
        }
        var room = (from r in _rooms where r.Id == id select r).FirstOrDefault();
        return room ?? (from r in _rooms where r.Id == "M-01" select r).First();
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

    // การจองที่ผู้ใช้คนนี้เห็น (ข้อมูลตัวอย่าง + ที่ตัวเองจอง) ใหม่สุดขึ้นก่อน
    private List<MyBookingViewModel> GetMyBookings()
    {
        var username = HttpContext.Session.GetString("Username");
        return (from b in _bookings
                where b.Username == "" || b.Username == username
                orderby b.Id descending
                select b).ToList();
    }

    // ===================== หน้า =====================

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Search()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        return View(_rooms);
    }

    public IActionResult RoomDetail(string id = "M-01")
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        var room = FindRoom(id);
        ViewBag.Reviews = (from r in _reviews
                           where r.RoomId == "" || r.RoomId == room.Id
                           select r).Reverse().ToList();
        return View(room);
    }

    [HttpPost]
    public IActionResult AddReview(ReviewViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(data.Comment))
        {
            TempData["Error"] = "กรุณาพิมพ์ความคิดเห็นก่อนส่งรีวิว";
            return RedirectToAction("RoomDetail", new { id = data.RoomId });
        }

        ReviewViewModel review = new();
        review.RoomId = data.RoomId;
        review.Name = HttpContext.Session.GetString("FullName") ?? "คุณ";
        review.Stars = Math.Clamp(data.Stars, 1, 5);
        review.Date = "เมื่อสักครู่";
        review.Comment = data.Comment.Trim();
        _reviews.Add(review);

        TempData["Success"] = "ส่งรีวิวแล้ว ขอบคุณสำหรับความคิดเห็น";
        return RedirectToAction("RoomDetail", new { id = data.RoomId });
    }

    [HttpGet]
    public IActionResult BookRoom(string id = "M-01", DateTime? startDate = null, int months = 6)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        var room = FindRoom(id);
        ViewBag.Room = room;

        // ค่าเริ่มต้นของฟอร์ม (วันเริ่มเช่าและระยะสัญญาอาจส่งมาจากหน้า RoomDetail)
        BookingViewModel data = new();
        data.RoomId = room.Id;
        data.StartDate = startDate != null && startDate.Value.Date >= DateTime.Today ? startDate.Value.Date : DateTime.Today;
        data.Months = months;
        data.ContactName = HttpContext.Session.GetString("FullName");
        data.Phone = HttpContext.Session.GetString("Phone");
        data.Email = HttpContext.Session.GetString("Email");
        data.Address = HttpContext.Session.GetString("Address");
        return View(data);
    }

    [HttpPost]
    public IActionResult BookRoom(BookingViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        var room = FindRoom(data.RoomId ?? "M-01");
        ViewBag.Room = room;

        if (!room.Free)
        {
            ViewBag.Error = "ห้องนี้มีผู้เช่าแล้ว จองไม่ได้";
            return View(data);
        }

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

        if (!data.AgreeTerms)
        {
            ViewBag.Error = "กรุณายอมรับเงื่อนไขสัญญาเช่าก่อนยืนยัน";
            return View(data);
        }

        // สร้างการจองใหม่ (เก็บในหน่วยความจำ ยังไม่บันทึกลงฐานข้อมูล)
        var start = data.StartDate.Date;
        var end = GetEndDate(start, data.Months);

        MyBookingViewModel booking = new();
        booking.Id = "BK-" + DateTime.Now.Year + "-" + (_bookings.Count + 1).ToString("000");
        booking.Username = HttpContext.Session.GetString("Username")!;
        booking.Room = room.Id;
        booking.Zone = room.Zone;
        booking.Period = start.ToString("d MMM yyyy", _th) + " – " + end.ToString("d MMM yyyy", _th);
        booking.Months = data.Months;
        booking.Rate = room.Price;
        booking.Status = "pending";
        booking.BookedOn = DateTime.Now.ToString("d MMM yyyy", _th);
        _bookings.Add(booking);

        TempData["Success"] = "จองห้อง " + room.Id + " แล้ว (" + booking.Id + ") กรุณาชำระเงินเพื่อยืนยันการจอง";
        return RedirectToAction("Profile", new { tab = "payment", bookingId = booking.Id });
    }

    public IActionResult Profile(string tab = "bookings", string? bookingId = null)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        if (tab != "bookings" && tab != "payment" && tab != "info")
        {
            tab = "bookings";
        }
        ViewBag.ActiveTab = tab;

        var bookings = GetMyBookings();
        ViewBag.Bookings = bookings;
        ViewBag.Payments = _payments;

        // การจองที่ต้องชำระ: ตามเลขที่ส่งมา ถ้าไม่มีใช้รายการรอชำระรายการแรก
        var due = (from b in bookings where b.Id == bookingId && b.Status == "pending" select b).FirstOrDefault()
                  ?? (from b in bookings where b.Status == "pending" select b).FirstOrDefault();
        ViewBag.DueBooking = due;
        if (due != null)
        {
            var depositMonths = GetDepositMonths(due.Months);
            ViewBag.DueDepositMonths = depositMonths;
            ViewBag.DueDeposit = due.Rate * depositMonths;
            ViewBag.DueTotal = due.Rate + due.Rate * depositMonths;
        }

        // ข้อมูลส่วนตัวที่บันทึกไว้ใน Session
        ProfileViewModel info = new();
        info.Fullname = HttpContext.Session.GetString("FullName") ?? "";
        info.Company = HttpContext.Session.GetString("Company") ?? "";
        info.Phone = HttpContext.Session.GetString("Phone") ?? "";
        info.Email = HttpContext.Session.GetString("Email") ?? "";
        info.TaxId = HttpContext.Session.GetString("TaxId") ?? "";
        info.Address = HttpContext.Session.GetString("Address") ?? "";
        info.SavedAt = HttpContext.Session.GetString("ProfileSavedAt") ?? "";
        return View(info);
    }

    [HttpPost]
    public IActionResult Pay(PaymentViewModel data)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }

        var booking = (from b in GetMyBookings() where b.Id == data.BookingId select b).FirstOrDefault();
        if (booking == null || booking.Status != "pending")
        {
            TempData["Error"] = "ไม่พบรายการที่ต้องชำระ";
            return RedirectToAction("Profile", new { tab = "payment" });
        }

        if (data.PayMethod == "transfer" && (data.Slip == null || data.Slip.Length == 0))
        {
            TempData["Error"] = "กรุณาแนบสลิปโอนเงิน";
            return RedirectToAction("Profile", new { tab = "payment", bookingId = data.BookingId });
        }

        // ยังไม่ต่อระบบชำระเงินจริง: เปลี่ยนสถานะเป็นรอตรวจสอบ
        booking.Status = "checking";
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

        if (string.IsNullOrWhiteSpace(data.Fullname) || string.IsNullOrWhiteSpace(data.Phone) || string.IsNullOrWhiteSpace(data.Email))
        {
            TempData["Error"] = "กรุณากรอกชื่อ เบอร์โทร และอีเมล";
            return RedirectToAction("Profile", new { tab = "info" });
        }

        // ยังไม่บันทึกลงตาราง user: เก็บไว้ใน Session ก่อน
        HttpContext.Session.SetString("FullName", data.Fullname.Trim());
        HttpContext.Session.SetString("Company", data.Company?.Trim() ?? "");
        HttpContext.Session.SetString("Phone", data.Phone.Trim());
        HttpContext.Session.SetString("Email", data.Email.Trim());
        HttpContext.Session.SetString("TaxId", data.TaxId?.Trim() ?? "");
        HttpContext.Session.SetString("Address", data.Address?.Trim() ?? "");
        HttpContext.Session.SetString("ProfileSavedAt", DateTime.Now.ToString("d MMM yyyy HH:mm", _th) + " น.");

        TempData["Success"] = "บันทึกข้อมูลแล้ว";
        return RedirectToAction("Profile", new { tab = "info" });
    }

    public IActionResult MyBookings()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        return RedirectToAction("Profile", new { tab = "bookings" });
    }

    public IActionResult Payment(string? bookingId = null)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        return RedirectToAction("Profile", new { tab = "payment", bookingId });
    }

    public IActionResult Reviews()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        return RedirectToAction("RoomDetail", new { id = "M-01" });
    }
}
