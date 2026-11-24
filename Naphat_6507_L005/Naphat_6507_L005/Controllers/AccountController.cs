using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class AccountController : Controller
{
    private readonly Csi402dbContext _db;

    // รับตัวเชื่อมฐานข้อมูลมาเก็บไว้ใน _db
    public AccountController(Csi402dbContext db)
    {
        _db = db;
    }

    // GET แสดงฟอร์มเข้าสู่ระบบ ถ้าล็อกอินอยู่แล้ว Redirect ตาม Role
    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("Role") != null)
            return RedirectByRole(HttpContext.Session.GetString("Role")!);
        return View();
    }

    // POST ค้นหาผู้ใช้ในตาราง user ที่ Username และ Password ตรงกัน ถ้าบัญชีถูกระงับหรือไม่พบ แสดง ViewBag.Error ถ้าพบ บันทึก Session แล้ว Redirect ตาม Role
    [HttpPost]
    public IActionResult Login(LoginViewModel data)
    {
        // ตรวจสอบจากตาราง user (ทั้งลูกค้าและพนักงานทุกฝ่าย)
        var dbUser = (from u in _db.Users
                      where u.Username == data.Username && u.PasswordHash == data.Password
                      select u).FirstOrDefault();
        if (dbUser != null && dbUser.IsActive == false)
        {
            ViewBag.Error = "บัญชีนี้ถูกระงับการใช้งาน กรุณาติดต่อผู้ดูแลระบบ";
            return View(data);
        }
        if (dbUser != null)
        {
            HttpContext.Session.SetString("Username", dbUser.Username);
            HttpContext.Session.SetString("Role", dbUser.Role);
            HttpContext.Session.SetString("FullName", dbUser.Fullname);
            return RedirectByRole(dbUser.Role);
        }

        ViewBag.Error = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
        return View(data);
    }

    // GET แสดงฟอร์มสมัครสมาชิกเปล่า
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST ตรวจว่ากรอกครบ รหัสผ่านตรงกัน เบอร์โทรถูกหลัก และชื่อผู้ใช้ไม่ซ้ำ แล้ว INSERT ลงตาราง user และ Redirect ไปหน้า Login
    [HttpPost]
    public IActionResult Register(RegisterViewModel data)
    {
        if (string.IsNullOrWhiteSpace(data.Username) || string.IsNullOrWhiteSpace(data.Password))
        {
            ViewBag.Error = "กรุณากรอกชื่อผู้ใช้และรหัสผ่าน";
            return View(data);
        }

        if (data.Password != data.ConfirmPassword)
        {
            ViewBag.Error = "รหัสผ่านและยืนยันรหัสผ่านไม่ตรงกัน";
            return View(data);
        }

        // เบอร์โทร (ไม่บังคับ) ถ้ากรอกต้องถูกต้อง
        var phone = CustomerController.DigitsOnly(data.Phone);
        if (!string.IsNullOrEmpty(phone) && CustomerController.PhoneError(phone) != null)
        {
            ViewBag.Error = CustomerController.PhoneError(phone);
            return View(data);
        }

        // ตรวจสอบซ้ำใน DB
        var exists = (from u in _db.Users where u.Username == data.Username select u).FirstOrDefault();
        if (exists != null)
        {
            ViewBag.Error = "ชื่อผู้ใช้งานนี้มีอยู่ในระบบแล้ว";
            return View(data);
        }

        // บันทึกลง DB table: user
        User newUser = new();
        newUser.Username = data.Username;
        newUser.PasswordHash = data.Password;
        newUser.Fullname = string.IsNullOrWhiteSpace(data.Fullname) ? data.Username : data.Fullname;
        newUser.Phone = CustomerController.DigitsOnly(data.Phone);   // พิมพ์เว้นวรรค/มีขีดมาได้ เก็บแต่ตัวเลข
        newUser.Role = "Customer";
        newUser.IsActive = true;
        newUser.CreatedAt = DateTime.Now;
        _db.Users.Add(newUser);
        _db.SaveChanges();

        TempData["Success"] = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบด้วยบัญชีของคุณ";
        return RedirectToAction("Login");
    }

    // ล้าง Session แล้วกลับหน้าหลัก
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Dashboard", "Customer");
    }

    // ส่งผู้ใช้ไปหน้าแรกตาม Role
    private IActionResult RedirectByRole(string role) => role switch
    {
        "Customer"        => RedirectToAction("Dashboard", "Customer"),
        "Accounting"      => RedirectToAction("Dashboard", "Accounting"),
        "Inventory"       => RedirectToAction("Dashboard", "Inventory"),
        "CustomerService" => RedirectToAction("Dashboard", "CustomerService"),
        "Manager"         => RedirectToAction("Dashboard", "Manager"),
        "Admin"           => RedirectToAction("Users", "Admin"),
        _                 => RedirectToAction("Login")
    };
}
