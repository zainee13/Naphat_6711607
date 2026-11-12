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

    // เข้าได้เฉพาะผู้ดูแลระบบ (ทุก action ต้องเช็คก่อน)
    private bool IsAdmin => HttpContext.Session.GetString("Role") == "Admin";

    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้ดูแลระบบ";
        ViewBag.Role = "Admin";
    }

    public IActionResult Dashboard()
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        return View();
    }

    public IActionResult Users()
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        var users = (from u in _db.Users select u).ToList();
        return View(users);
    }

    // หน้าแก้ไขข้อมูลผู้ใช้ (GET) - แบบเดียวกับ Lab10
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
    [HttpPost]
    public IActionResult EditUser(User data)
    {
        if (!IsAdmin)
        {
            return RedirectToAction("Login", "Account");
        }

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
            TempData["Success"] = "บันทึกข้อมูลผู้ใช้ " + user.Username + " แล้ว";
        }
        return RedirectToAction("Users");
    }

    // ลบข้อมูลผู้ใช้ (POST เท่านั้น) - แบบเดียวกับ Lab10_1
    // ถ้าผู้ใช้มีข้อมูลผูกอยู่ (การจอง, การชำระที่ตรวจ, ใบเสร็จ, การคืนเงิน, สต็อก) ลบไม่ได้ ให้ระงับการใช้งานแทน
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

        bool inUse = (from b in _db.Bookings where b.UserId == user.UserId select b).Any()
                     || (from p in _db.Payments where p.VerifiedBy == user.UserId select p).Any()
                     || (from i in _db.Invoices where i.IssuedBy == user.UserId select i).Any()
                     || (from r in _db.Receipts where r.IssuedBy == user.UserId select r).Any()
                     || (from rf in _db.Refunds where rf.ProcessedBy == user.UserId select rf).Any()
                     || (from s in _db.Stockitems where s.UpdatedBy == user.UserId select s).Any();
        if (inUse)
        {
            TempData["Error"] = "ลบ " + user.Username + " ไม่ได้ เพราะมีข้อมูลการจองหรือเอกสารผูกอยู่ ให้แก้ไขเป็น \"ระงับการใช้งาน\" แทน";
            return RedirectToAction("Users");
        }

        _db.Users.Remove(user);
        _db.SaveChanges();
        TempData["Success"] = "ลบผู้ใช้ " + user.Username + " แล้ว";
        return RedirectToAction("Users");
    }
}
