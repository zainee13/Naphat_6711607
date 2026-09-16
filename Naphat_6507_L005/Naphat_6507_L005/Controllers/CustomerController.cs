using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class CustomerController : Controller
{
    private bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    private void SetUserBag()
    {
        var username = HttpContext.Session.GetString("Username");
        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(username);
        ViewBag.Username = username;
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้เยี่ยมชม";
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "Guest";
    }

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Search()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        return View();
    }

    public IActionResult RoomDetail(string id = "M-01")
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        ViewBag.RoomId = id;
        return View();
    }

    public IActionResult BookRoom(string id = "M-01")
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        ViewBag.RoomId = id;
        return View();
    }

    public IActionResult Profile(string tab = "bookings", string bookingId = "BK-2026-002")
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();
        ViewBag.ActiveTab = tab;
        ViewBag.BookingId = bookingId;
        return View();
    }

    public IActionResult MyBookings()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        return RedirectToAction("Profile", new { tab = "bookings" });
    }

    public IActionResult Payment(string bookingId = "BK-2026-002")
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        return RedirectToAction("Profile", new { tab = "payment", bookingId });
    }

    public IActionResult Reviews()
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction("Login", "Account");
        }
        return RedirectToAction("RoomDetail", new { id = "M-01" });
    }
}
