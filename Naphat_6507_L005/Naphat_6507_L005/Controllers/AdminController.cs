using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class AdminController : Controller
{
    private readonly Csi402dbContext _db;
    // รับตัวเชื่อมฐานข้อมูลมาเก็บไว้ใน _db
    public AdminController(Csi402dbContext db)
    {
        _db = db;
    }

    // เข้าได้เฉพาะผู้ดูแลระบบ (ทุก action ต้องเช็คก่อน)
    private bool IsAdmin => AccountController.IsActiveUser(_db, HttpContext) && HttpContext.Session.GetString("Role") == "Admin";

    // ส่งชื่อและ Role ผ่าน ViewBag ให้ Layout แสดง
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้ดูแลระบบ";
        ViewBag.Role = "Admin";
    }

    // บทบาททั้งหมดในระบบ (ชื่อที่แสดงในหน้าจอ)
    public static readonly Dictionary<string, string> RoleText = new()
    {
        { "Customer", "ลูกค้า" },
        { "Accounting", "ฝ่ายบัญชี" },
        { "Inventory", "ฝ่ายคลังสินค้า" },
        { "CustomerService", "ฝ่ายบริการลูกค้า" },
        { "Manager", "ผู้จัดการ" },
        { "Admin", "ผู้ดูแลระบบ" },
    };

    // หน้าแรกของ Admin คือจัดการผู้ใช้ (ไม่มีหน้าภาพรวมแล้ว) ลิงก์เก่าที่มาที่ Dashboard ส่งต่อไปหน้า Users
    public IActionResult Dashboard()
    {
        return RedirectToAction("Users");
    }

    // รายชื่อผู้ใช้ ค้นหาด้วยชื่อผู้ใช้/ชื่อ/อีเมล (?q=) และกรองตามบทบาท (?role=)
    // GET อ่านตาราง user ค้นหาด้วย q และกรองด้วย role แล้วส่ง List<User> ไปที่ View
    public IActionResult Users(string? q, string? role)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        // ตัวเลขสรุป (นับจากผู้ใช้ทั้งหมด ไม่ขึ้นกับตัวกรอง)
        var all = (from u in _db.Users select u).ToList();
        ViewBag.CountAll = all.Count;
        ViewBag.CountActive = all.Count(u => u.IsActive == true);
        ViewBag.CountCustomer = all.Count(u => u.Role == "Customer");
        ViewBag.CountStaff = all.Count(u => u.Role != "Customer");
        ViewBag.CountInactive = all.Count(u => u.IsActive != true);

        // การเช่าของลูกค้า: การจองที่ยังไม่จบ (รอชำระ / รอตรวจ / ใช้งานอยู่) จัดกลุ่มตาม UserId
        var statusText = new Dictionary<string, string>
        {
            { "Pending", "รอชำระ" },
            { "Checking", "รอตรวจการชำระ" },
            { "Active", "กำลังเช่า" },
        };
        var bookings = (from b in _db.Bookings
                        where b.BookingStatus == "Pending" || b.BookingStatus == "Checking" || b.BookingStatus == "Active"
                        orderby b.RoomId
                        select b).ToList();
        Dictionary<int, List<UserRentalViewModel>> rentals = new();
        foreach (var b in bookings)
        {
            UserRentalViewModel item = new();
            item.RoomId = b.RoomId;
            item.BookingId = b.BookingId;
            item.Status = b.BookingStatus;
            item.StatusText = statusText[b.BookingStatus];
            if (!rentals.ContainsKey(b.UserId))
            {
                rentals[b.UserId] = new List<UserRentalViewModel>();
            }
            rentals[b.UserId].Add(item);
        }
        ViewBag.Rentals = rentals;
        ViewBag.CountRenting = (from b in bookings where b.BookingStatus == "Active" select b.UserId).Distinct().Count();

        var keyword = (q ?? "").Trim();
        if (role != null && !RoleText.ContainsKey(role))
        {
            role = null;
        }
        ViewBag.Q = keyword;
        ViewBag.FilterRole = role;

        var users = (from u in all
                     where (keyword == "" || u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                          || u.Fullname.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                          || (u.Email ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                           && (role == null || u.Role == role)
                     orderby u.Role == "Customer", u.Role, u.Username
                     select u).ToList();
        return View(users);
    }

    // เพิ่มผู้ใช้ใหม่ (GET) - ฟอร์มเปล่า
    [HttpGet]
    public IActionResult AddUser()
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        return View(new AddUserViewModel());
    }

    // เพิ่มผู้ใช้ใหม่ (POST) - ตรวจข้อมูลแล้วบันทึกลงตาราง user
    // POST ตรวจกรอกครบ รหัสผ่านตรงกัน เบอร์ถูกหลัก ชื่อไม่ซ้ำ และ Role ถูกต้อง แล้ว INSERT ลงตาราง user
    [HttpPost]
    public IActionResult AddUser(AddUserViewModel data)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        if (string.IsNullOrWhiteSpace(data.Username) || string.IsNullOrWhiteSpace(data.Password) || string.IsNullOrWhiteSpace(data.Fullname))
        {
            ViewBag.Error = "กรุณากรอกชื่อผู้ใช้ รหัสผ่าน และชื่อ-นามสกุล";
            return View(data);
        }
        if (data.Password != data.ConfirmPassword)
        {
            ViewBag.Error = "รหัสผ่านและยืนยันรหัสผ่านไม่ตรงกัน";
            return View(data);
        }
        if (!RoleText.ContainsKey(data.Role))
        {
            ViewBag.Error = "กรุณาเลือกบทบาท";
            return View(data);
        }

        var phone = CustomerController.DigitsOnly(data.Phone);
        if (!string.IsNullOrEmpty(phone) && CustomerController.PhoneError(phone) != null)
        {
            ViewBag.Error = CustomerController.PhoneError(phone);
            return View(data);
        }

        var username = data.Username.Trim();
        var exists = (from u in _db.Users where u.Username == username select u).Any();
        if (exists)
        {
            ViewBag.Error = "ชื่อผู้ใช้ " + username + " มีอยู่ในระบบแล้ว";
            return View(data);
        }

        User user = new();
        user.Username = username;
        user.PasswordHash = data.Password;
        user.Fullname = data.Fullname.Trim();
        user.Phone = CustomerController.DigitsOnly(data.Phone);   // พิมพ์เว้นวรรค/มีขีดมาได้ เก็บแต่ตัวเลข
        user.Email = string.IsNullOrWhiteSpace(data.Email) ? null : data.Email.Trim();
        user.Role = data.Role;
        user.IsActive = true;
        user.CreatedAt = DateTime.Now;
        _db.Users.Add(user);
        _db.SaveChanges();

        TempData["Success"] = "เพิ่มผู้ใช้ " + user.Username + " (" + RoleText[user.Role] + ") แล้ว";
        return RedirectToAction("Users");
    }

    // หน้าแก้ไขข้อมูลผู้ใช้ (GET) - แบบเดียวกับ Lab10
    // GET อ่านผู้ใช้ตาม id มาแสดงในฟอร์ม
    public IActionResult EditUser(int? id)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        var user = (from u in _db.Users where u.UserId == id select u).FirstOrDefault();
        if (user == null)
        {
            TempData["Error"] = "ไม่พบผู้ใช้นี้";
            return RedirectToAction("Users");
        }
        return View(user);
    }

    // บันทึกการแก้ไขข้อมูลผู้ใช้ (POST) - แบบเดียวกับ Lab10
    // POST ตรวจเบอร์โทรและชื่อไม่ซ้ำ แล้ว UPDATE ตาราง user
    [HttpPost]
    public IActionResult EditUser(User data)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }

        var phone = CustomerController.DigitsOnly(data.Phone);
        if (!string.IsNullOrEmpty(phone) && CustomerController.PhoneError(phone) != null)
        {
            TempData["Error"] = CustomerController.PhoneError(phone);
            return RedirectToAction("EditUser", new { id = data.UserId });
        }

        var user = (from u in _db.Users where u.UserId == data.UserId select u).FirstOrDefault();
        if (user != null)
        {
            // ชื่อผู้ใช้ต้องไม่ซ้ำกับคนอื่น
            var exists = (from u in _db.Users where u.Username == data.Username && u.UserId != data.UserId select u).Any();
            if (exists)
            {
                TempData["Error"] = "ชื่อผู้ใช้ " + data.Username + " มีอยู่ในระบบแล้ว";
                return RedirectToAction("EditUser", new { id = data.UserId });
            }

            // บัญชีที่ล็อกอินอยู่: ระงับหรือเปลี่ยนบทบาทตัวเองไม่ได้ (กันเข้าระบบไม่ได้)
            bool isSelf = user.Username == HttpContext.Session.GetString("Username");
            if (isSelf && (data.IsActive != true || data.Role != "Admin"))
            {
                TempData["Error"] = "ระงับหรือเปลี่ยนบทบาทบัญชีที่กำลังใช้งานอยู่ไม่ได้";
                return RedirectToAction("EditUser", new { id = data.UserId });
            }

            user.Username = data.Username;
            user.Fullname = data.Fullname;
            user.Email = data.Email;
            user.Phone = CustomerController.DigitsOnly(data.Phone);   // หน้าเว็บเว้นวรรค เก็บแต่ตัวเลข
            user.Role = data.Role;
            user.IsActive = data.IsActive;

            // ถ้ามีการเปลี่ยนรหัสผ่าน
            if (!string.IsNullOrEmpty(data.PasswordHash))
            {
                user.PasswordHash = data.PasswordHash;
            }

            _db.SaveChanges();
            if (isSelf)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.Fullname);
            }
            TempData["Success"] = "บันทึกข้อมูลผู้ใช้ " + user.Username + " แล้ว";
        }
        return RedirectToAction("Users");
    }

    // ลบข้อมูลผู้ใช้ (POST เท่านั้น) - แบบเดียวกับ Lab10_1
    // ถ้าผู้ใช้มีข้อมูลผูกอยู่ (การจอง, การชำระที่ตรวจ, ใบเสร็จ, การคืนเงิน, สต็อก, เรื่องแจ้งปัญหา) ลบไม่ได้ ให้ระงับการใช้งานแทน
    // POST ลบเฉพาะผู้ใช้ที่ไม่มีข้อมูลผูกอยู่ และไม่ใช่บัญชีตัวเอง
    [HttpPost]
    public IActionResult DeleteUser(int? id)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = (from u in _db.Users where u.UserId == id select u).FirstOrDefault();
        if (user == null)
        {
            TempData["Error"] = "ไม่พบผู้ใช้นี้";
            return RedirectToAction("Users");
        }

        // ลบบัญชีที่กำลังใช้งานอยู่ไม่ได้ (กันเข้าระบบไม่ได้)
        if (user.Username == HttpContext.Session.GetString("Username"))
        {
            TempData["Error"] = "ลบบัญชีที่กำลังใช้งานอยู่ไม่ได้";
            return RedirectToAction("Users");
        }

        bool inUse = (from b in _db.Bookings where b.UserId == user.UserId select b).Any()
                     || (from p in _db.Payments where p.VerifiedBy == user.UserId select p).Any()
                     || (from i in _db.Invoices where i.IssuedBy == user.UserId select i).Any()
                     || (from r in _db.Receipts where r.IssuedBy == user.UserId select r).Any()
                     || (from rf in _db.Refunds where rf.ProcessedBy == user.UserId select rf).Any()
                     || (from s in _db.Stockitems where s.UpdatedBy == user.UserId select s).Any()
                     || (from t in _db.Supporttickets where t.UserId == user.UserId || t.AssignedTo == user.UserId select t).Any();
        if (inUse)
        {
            TempData["Error"] = "ลบ " + user.Username + " ไม่ได้ เพราะมีข้อมูลการจอง เอกสาร หรือเรื่องแจ้งปัญหาผูกอยู่ ให้แก้ไขเป็น \"ระงับการใช้งาน\" แทน";
            return RedirectToAction("Users");
        }

        _db.Users.Remove(user);
        _db.SaveChanges();
        TempData["Success"] = "ลบผู้ใช้ " + user.Username + " แล้ว";
        return RedirectToAction("Users");
    }

    // ===================== โค้ดส่วนลด (ตาราง promocode) =====================
    // Admin สร้าง / แก้ / เปิด-ปิด / ลบโค้ด ส่วนการคิดส่วนลดตอนลูกค้าจองอยู่ที่ CustomerController.CheckPromo

    private static readonly System.Globalization.CultureInfo _th = new("th-TH");

    // รายการโค้ดทั้งหมด พร้อมสถานะและจำนวนการจองที่ใช้
    // GET อ่านทุกโค้ดจาก promocode นับการใช้จาก booking ที่ไม่ถูกยกเลิก และคำนวณสถานะ
    public IActionResult Promos()
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var promos = (from p in _db.Promocodes orderby p.CreatedAt descending select p).ToList();
        List<PromoListItemViewModel> list = new();
        foreach (var p in promos)
        {
            PromoListItemViewModel item = new();
            item.Code = p.Code;
            item.Description = p.Description;
            item.DiscountText = p.DiscountType == "Percent" ? p.DiscountValue.ToString("0.##") + "%" : "฿" + p.DiscountValue.ToString("N0");
            item.MinMonths = p.MinMonths;
            item.PeriodText = (p.StartDate?.ToString("d MMM yyyy", _th) ?? "ไม่กำหนด") + " – " + (p.EndDate?.ToString("d MMM yyyy", _th) ?? "ไม่กำหนด");
            item.IsActive = p.IsActive;
            item.StatusText = !p.IsActive ? "ปิดใช้งาน"
                            : p.EndDate != null && today > p.EndDate ? "หมดอายุ"
                            : p.StartDate != null && today < p.StartDate ? "ยังไม่เริ่ม"
                            : "ใช้งานได้";
            item.UsedCount = (from b in _db.Bookings where b.PromoCode == p.Code && b.BookingStatus != "Cancelled" select b).Count();
            item.UsedText = item.UsedCount + (p.MaxUses == null ? " ครั้ง (ไม่จำกัด)" : " / " + p.MaxUses + " ครั้ง");
            if (p.IsActive && p.MaxUses != null && item.UsedCount >= p.MaxUses)
            {
                item.StatusText = "ใช้ครบแล้ว";
            }
            list.Add(item);
        }
        return View(list);
    }

    // ตรวจข้อมูลฟอร์มโค้ด คืนข้อความผิดพลาด (null = ผ่าน)
    private static string? ValidatePromo(PromoFormViewModel data)
    {
        if (string.IsNullOrWhiteSpace(data.Code))
        {
            return "กรุณากรอกโค้ด";
        }
        if (data.Code.Trim().Length > 30 || !data.Code.Trim().All(char.IsLetterOrDigit))
        {
            return "โค้ดต้องเป็นตัวอักษรภาษาอังกฤษหรือตัวเลข ไม่เกิน 30 ตัว ไม่มีเว้นวรรค";
        }
        if (data.DiscountType != "Percent" && data.DiscountType != "Amount")
        {
            return "กรุณาเลือกประเภทส่วนลด";
        }
        if (data.DiscountValue <= 0 || (data.DiscountType == "Percent" && data.DiscountValue > 100))
        {
            return data.DiscountType == "Percent" ? "ส่วนลดต้องอยู่ระหว่าง 1–100%" : "ส่วนลดต้องมากกว่า 0 บาท";
        }
        if ((data.MaxUses != null && data.MaxUses < 1) || (data.MaxUsesPerUser != null && data.MaxUsesPerUser < 1))
        {
            return "จำนวนครั้งต้องเป็น 1 ขึ้นไป หรือเว้นว่างถ้าไม่จำกัด";
        }
        if (data.MaxUses != null && data.MaxUsesPerUser != null && data.MaxUsesPerUser > data.MaxUses)
        {
            return "จำนวนครั้งต่อคนต้องไม่มากกว่าจำนวนครั้งทั้งหมด";
        }
        if (data.MinMonths < 1 || data.MinMonths > 12)
        {
            return "สัญญาขั้นต่ำต้องอยู่ระหว่าง 1–12 เดือน";
        }
        if (data.StartDate != null && data.EndDate != null && data.EndDate < data.StartDate)
        {
            return "วันหมดอายุต้องไม่ก่อนวันเริ่ม";
        }
        return null;
    }

    // คัดลอกค่าจากฟอร์มลงแถวในตาราง (ใช้ทั้งเพิ่มและแก้ไข)
    private static void CopyPromo(PromoFormViewModel data, Promocode promo)
    {
        promo.Description = string.IsNullOrWhiteSpace(data.Description) ? null : data.Description.Trim();
        promo.DiscountType = data.DiscountType;
        promo.DiscountValue = data.DiscountValue;
        promo.MinMonths = data.MinMonths;
        promo.MaxUses = data.MaxUses;
        promo.MaxUsesPerUser = data.MaxUsesPerUser;
        promo.StartDate = data.StartDate == null ? null : DateOnly.FromDateTime(data.StartDate.Value);
        promo.EndDate = data.EndDate == null ? null : DateOnly.FromDateTime(data.EndDate.Value);
        promo.IsActive = data.IsActive;
    }

    // เพิ่มโค้ด (GET)
    // GET แสดงฟอร์มเพิ่มโค้ดเปล่า
    [HttpGet]
    public IActionResult AddPromo()
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        return View("PromoForm", new PromoFormViewModel());
    }

    // เพิ่มโค้ด (POST)
    // POST ตรวจด้วย ValidatePromo และโค้ดไม่ซ้ำ แล้ว INSERT ลงตาราง promocode
    [HttpPost]
    public IActionResult AddPromo(PromoFormViewModel data)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        data.IsEdit = false;

        var error = ValidatePromo(data);
        if (error != null)
        {
            ViewBag.Error = error;
            return View("PromoForm", data);
        }
        var code = data.Code!.Trim().ToUpper();
        if ((from p in _db.Promocodes where p.Code == code select p).Any())
        {
            ViewBag.Error = "มีโค้ด " + code + " อยู่แล้ว";
            return View("PromoForm", data);
        }

        Promocode promo = new();
        promo.Code = code;
        CopyPromo(data, promo);
        promo.CreatedAt = DateTime.Now;
        _db.Promocodes.Add(promo);
        _db.SaveChanges();

        TempData["Success"] = "เพิ่มโค้ด " + code + " แล้ว";
        return RedirectToAction("Promos");
    }

    // แก้ไขโค้ด (GET)
    // GET อ่านโค้ดเดิมมาแสดงในฟอร์ม
    [HttpGet]
    public IActionResult EditPromo(string id)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var promo = (from p in _db.Promocodes where p.Code == id select p).FirstOrDefault();
        if (promo == null)
        {
            TempData["Error"] = "ไม่พบโค้ด " + id;
            return RedirectToAction("Promos");
        }
        PromoFormViewModel data = new();
        data.Code = promo.Code;
        data.Description = promo.Description;
        data.DiscountType = promo.DiscountType;
        data.DiscountValue = promo.DiscountValue;
        data.MinMonths = promo.MinMonths;
        data.MaxUses = promo.MaxUses;
        data.MaxUsesPerUser = promo.MaxUsesPerUser;
        data.StartDate = promo.StartDate?.ToDateTime(TimeOnly.MinValue);
        data.EndDate = promo.EndDate?.ToDateTime(TimeOnly.MinValue);
        data.IsActive = promo.IsActive;
        data.IsEdit = true;
        return View("PromoForm", data);
    }

    // แก้ไขโค้ด (POST) เปลี่ยนตัวโค้ดไม่ได้ (การจองเก่าอ้างถึงโค้ดนี้อยู่)
    // POST ตรวจด้วย ValidatePromo แล้ว UPDATE ตาราง promocode
    [HttpPost]
    public IActionResult EditPromo(PromoFormViewModel data)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        data.IsEdit = true;

        var promo = (from p in _db.Promocodes where p.Code == data.Code select p).FirstOrDefault();
        if (promo == null)
        {
            TempData["Error"] = "ไม่พบโค้ด " + data.Code;
            return RedirectToAction("Promos");
        }
        var error = ValidatePromo(data);
        if (error != null)
        {
            ViewBag.Error = error;
            return View("PromoForm", data);
        }

        CopyPromo(data, promo);
        _db.SaveChanges();
        TempData["Success"] = "บันทึกโค้ด " + promo.Code + " แล้ว";
        return RedirectToAction("Promos");
    }

    // ลบโค้ด (POST) ถ้ามีการจองใช้โค้ดนี้แล้ว ลบไม่ได้ ให้ปิดใช้งานแทน (เก็บประวัติไว้)
    // POST ลบได้เฉพาะโค้ดที่ยังไม่มีการจองใช้
    [HttpPost]
    public IActionResult DeletePromo(string id)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }

        var promo = (from p in _db.Promocodes where p.Code == id select p).FirstOrDefault();
        if (promo == null)
        {
            TempData["Error"] = "ไม่พบโค้ด " + id;
            return RedirectToAction("Promos");
        }
        if ((from b in _db.Bookings where b.PromoCode == promo.Code select b).Any())
        {
            TempData["Error"] = "ลบโค้ด " + promo.Code + " ไม่ได้ เพราะมีการจองใช้โค้ดนี้แล้ว ให้แก้เป็น \"ปิดใช้งาน\" แทน";
            return RedirectToAction("Promos");
        }

        _db.Promocodes.Remove(promo);
        _db.SaveChanges();
        TempData["Success"] = "ลบโค้ด " + promo.Code + " แล้ว";
        return RedirectToAction("Promos");
    }
}
