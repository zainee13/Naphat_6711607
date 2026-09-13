using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class CustomerController : Controller
{
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ลูกค้า";
        ViewBag.Role = "Customer";
    }

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Search()
    {
        SetUserBag();
        return View();
    }

    public IActionResult RoomDetail(string id = "M-01")
    {
        SetUserBag();
        ViewBag.RoomId = id;
        return View();
    }

    public IActionResult BookRoom(string id = "M-01")
    {
        SetUserBag();
        ViewBag.RoomId = id;
        return View();
    }

    public IActionResult Profile(string tab = "bookings", string bookingId = "BK-2026-002")
    {
        SetUserBag();
        ViewBag.ActiveTab = tab;
        ViewBag.BookingId = bookingId;
        return View();
    }

    public IActionResult MyBookings()
    {
        return RedirectToAction("Profile", new { tab = "bookings" });
    }

    public IActionResult Payment(string bookingId = "BK-2026-002")
    {
        return RedirectToAction("Profile", new { tab = "payment", bookingId });
    }

    public IActionResult Reviews()
    {
        SetUserBag();
        return View();
    }
}
