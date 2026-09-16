using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class AccountController : Controller
{
    // Demo accounts for frontend simulation
    private static readonly Dictionary<string, (string password, string role, string name)> _users = new()
    {
        { "customer1",    ("1234", "Customer",        "นภัส นาวาหัสดินกุล") },
        { "accounting1",  ("1234", "Accounting",      "สมหญิง บัญชีดี") },
        { "inventory1",   ("1234", "Inventory",       "สมชาย คลังสินค้า") },
        { "cs1",          ("1234", "CustomerService", "วรรณิสา บริการ") },
        { "manager1",     ("1234", "Manager",         "ประยุทธ ผู้จัดการ") },
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
    public IActionResult Login(string username, string password)
    {
        if (_users.TryGetValue(username.ToLower(), out var info) && info.password == password)
        {
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetString("Role", info.role);
            HttpContext.Session.SetString("FullName", info.name);
            return RedirectByRole(info.role);
        }
        ViewBag.Error = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
        return View();
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
