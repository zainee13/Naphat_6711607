using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class AccountController : Controller
{
    private readonly Csi402dbContext _db;

    // Demo accounts for frontend simulation (staff accounts)
    public static readonly Dictionary<string, (string password, string role, string name)> _users = new()
    {
        { "accounting1",  ("1234", "Accounting",      "บัญชีดี") },
        { "inventory1",   ("1234", "Inventory",       "คลังสินค้า") },
        { "cs1",          ("1234", "CustomerService", "บริการ") },
        { "manager1",     ("1234", "Manager",         "ผู้จัดการ") },
        { "admin",        ("1234", "Admin",           "ผู้ดูแลระบบ") },
    };

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
        // ตรวจสอบ staff accounts ก่อน
        if (_users.TryGetValue(data.Username.ToLower(), out var info) && info.password == data.Password)
        {
            HttpContext.Session.SetString("Username", data.Username);
            HttpContext.Session.SetString("Role", info.role);
            HttpContext.Session.SetString("FullName", info.name);
            return RedirectByRole(info.role);
        }

        // ตรวจสอบจาก DB (users ที่ Register)
        var dbUser = (from u in _db.Users
                      where u.Username == data.Username && u.PasswordHash == data.Password
                      select u).FirstOrDefault();
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

        // ตรวจสอบซ้ำใน DB
        var exists = (from u in _db.Users where u.Username == data.Username select u).FirstOrDefault();
        if (exists != null || _users.ContainsKey(data.Username.ToLower()))
        {
            ViewBag.Error = "ชื่อผู้ใช้งานนี้มีอยู่ในระบบแล้ว";
            return View(data);
        }

        // บันทึกลง DB table: user
        User newUser = new();
        newUser.Username = data.Username;
        newUser.PasswordHash = data.Password;
        newUser.Fullname = string.IsNullOrWhiteSpace(data.Fullname) ? data.Username : data.Fullname;
        newUser.Phone = data.Phone;
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
        "Admin"           => RedirectToAction("Dashboard", "Admin"),
        _                 => RedirectToAction("Login")
    };
}
