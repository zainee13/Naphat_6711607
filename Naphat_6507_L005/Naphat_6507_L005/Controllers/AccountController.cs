using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class AccountController : Controller
{
    // Demo accounts for frontend simulation
    public static readonly Dictionary<string, (string password, string role, string name)> _users = new()
    {
        { "customer1",    ("1234", "Customer",        "นภัส นาวาหัสดินกุล") },
        { "accounting1",  ("1234", "Accounting",      "บัญชีดี") },
        { "inventory1",   ("1234", "Inventory",       "คลังสินค้า") },
        { "cs1",          ("1234", "CustomerService", "บริการ") },
        { "manager1",     ("1234", "Manager",         "ผู้จัดการ") },
        { "admin",        ("1234", "Admin",           "ผู้ดูแลระบบ") },
    };

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
        if (_users.TryGetValue(data.Username.ToLower(), out var info) && info.password == data.Password)
        {
            HttpContext.Session.SetString("Username", data.Username);
            HttpContext.Session.SetString("Role", info.role);
            HttpContext.Session.SetString("FullName", info.name);
            return RedirectByRole(info.role);
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

        if (_users.ContainsKey(data.Username.ToLower()))
        {
            ViewBag.Error = "ชื่อผู้ใช้งานนี้มีอยู่ในระบบแล้ว";
            return View(data);
        }

        _users[data.Username.ToLower()] = (data.Password, "Customer", string.IsNullOrWhiteSpace(data.Fullname) ? data.Username : data.Fullname);
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
