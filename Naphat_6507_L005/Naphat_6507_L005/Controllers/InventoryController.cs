using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class InventoryController : Controller
{
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "พนักงานคลัง";
        ViewBag.Role = "Inventory";
    }

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }

    public IActionResult StockList()
    {
        SetUserBag();
        return View();
    }

    public IActionResult AddStock()
    {
        SetUserBag();
        return View();
    }

    public IActionResult Alerts()
    {
        SetUserBag();
        return View();
    }
}
