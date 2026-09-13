using Microsoft.AspNetCore.Mvc;

namespace Naphat_6507_L005.Controllers;

public class ManagerController : Controller
{
    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "ผู้จัดการ";
        ViewBag.Role = "Manager";
    }

    public IActionResult Dashboard()
    {
        SetUserBag();
        return View();
    }
}
