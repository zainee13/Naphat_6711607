using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class AdminController : Controller
{
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้ดูแลระบบ";
        ViewBag.Role = "Admin";
    }

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Users()
    {
        SetUserBag();
        return View(AccountController._users);
    }
}
