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

    public IActionResult RoomDetail(string id = "BN-A01")
    {
        SetUserBag();
        ViewBag.RoomId = id;
        return View();
    }

    public IActionResult BookRoom(string id = "BN-A01")
    {
        SetUserBag();
        ViewBag.RoomId = id;
        return View();
    }

    public IActionResult MyBookings()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Payment(string bookingId = "BK-2026-001")
    {
        SetUserBag();
        ViewBag.BookingId = bookingId;
        return View();
    }

    public IActionResult Reviews()
    {
        SetUserBag();
        return View();
    }
}
