using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class CustomerServiceController : Controller
{
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "พนักงาน CS";
        ViewBag.Role = "CustomerService";
    }

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Tickets()
    {
        SetUserBag();
        return View();
    }

    public IActionResult TicketDetail(string id = "TK-001")
    {
        SetUserBag();
        ViewBag.TicketId = id;
        return View();
    }
}
