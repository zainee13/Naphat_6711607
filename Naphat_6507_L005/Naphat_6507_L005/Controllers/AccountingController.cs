using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class AccountingController : Controller
{
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "พนักงานบัญชี";
        ViewBag.Role = "Accounting";
    }

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Invoices()
    {
        SetUserBag();
        return View();
    }

    public IActionResult DailySummary()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Refunds()
    {
        SetUserBag();
        return View();
    }
}
