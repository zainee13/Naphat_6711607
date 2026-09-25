using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        
        return View();
    }

    public IActionResult Search()
    {
        return View();
    }

    public IActionResult Secirity()
    {
        return View("secirity");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Lab2()
    {
       return View();
    }
    public IActionResult Lab5(string Name, string LastName)
    {
        var Data = new Lab5ViewModels();
        Data.Name = Name;
        Data.LastName = LastName;
        return View(Data);
    }
    public IActionResult Lab5_2()
    {
        var User = new List<Lab5ViewModels>
        {
            new Lab5ViewModels{Name = "a",LastName = "b"},
            new Lab5ViewModels{Name = "c",LastName = "d"}
        };
        return View(User);
    }

    public IActionResult Lab5_3()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Lab5_3(Lab5ViewModels data)
    {
        string a, b, c;
        a = data.Name;
        b = data.LastName;
        // c = a + " " + b;
        c = data.Name + " " + data.LastName;
        ViewBag.Name = c;
        // return View(data);
        return RedirectToAction("Lab5", "Home", new { Name = data.Name, LastName = data.LastName });
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
