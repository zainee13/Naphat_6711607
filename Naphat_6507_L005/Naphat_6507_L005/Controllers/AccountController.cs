using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class AccountController : Controller
{
    private readonly Csi402dbContext _db;

    public AccountController(Csi402dbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("Role") != null)
            return RedirectByRole(HttpContext.Session.GetString("Role")!);
        return View();
    }

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

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

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

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Dashboard", "Customer");
    }

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
