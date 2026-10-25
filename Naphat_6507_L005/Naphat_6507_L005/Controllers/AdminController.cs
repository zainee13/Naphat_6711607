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

    // หน้าแก้ไขข้อมูลผู้ใช้ (GET) - แบบเดียวกับ Lab10
    public IActionResult EditUser(int? id)
    {
        SetUserBag();
        var user = (from u in _db.Users where u.UserId == id select u).FirstOrDefault();
        return View(user);
    }

    // บันทึกการแก้ไขข้อมูลผู้ใช้ (POST) - แบบเดียวกับ Lab10
    [HttpPost]
    public IActionResult EditUser(User data)
    {
        var user = (from u in _db.Users where u.UserId == data.UserId select u).FirstOrDefault();
        if (user != null)
        {
            user.Username = data.Username;
            user.Fullname = data.Fullname;
            user.Email = data.Email;
            user.Phone = data.Phone;
            user.Role = data.Role;
            user.IsActive = data.IsActive;

            // ถ้ามีการเปลี่ยนรหัสผ่าน
            if (!string.IsNullOrEmpty(data.PasswordHash))
            {
                user.PasswordHash = data.PasswordHash;
            }

            _db.SaveChanges();
        }
        return RedirectToAction("Users");
    }

    // ลบข้อมูลผู้ใช้ - แบบเดียวกับ Lab10_1
    public IActionResult DeleteUser(int? id)
    {
        var user = (from u in _db.Users where u.UserId == id select u).FirstOrDefault();
        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
        }
        return RedirectToAction("Users");
    }
}

