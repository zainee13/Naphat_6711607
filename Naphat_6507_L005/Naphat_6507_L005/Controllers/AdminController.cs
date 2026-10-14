using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;

namespace Naphat_6507_L005.Controllers;

public class AdminController : Controller
{
    private readonly Csi402dbContext _db;
    public AdminController(Csi402dbContext db)
    {
        _db = db;
    }

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
        var users = (from u in _db.Users select u).ToList();
        return View(users);
    }
}

