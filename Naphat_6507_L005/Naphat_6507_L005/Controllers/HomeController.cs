using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class HomeController : Controller
{
    private readonly Csi402dbContext _db;
    public HomeController(Csi402dbContext db)
    {
        _db = db;
    }

    public IActionResult Lab9()
    {
        //Create Insert Value in SQL
        return View();
    }
    [HttpPost]
    public IActionResult Lab9(Labstudent data)
    {
        if (data.StdPassword != null)
        {
            if (ModelState.IsValid)
            {
                var CheckStd = (from a in _db.Labstudents where a.StdId == data.StdId select a).FirstOrDefault();
                if (CheckStd == null)
                {
                    Labstudent s = new();
                    s.StdId = data.StdId;
                    s.StdPassword = data.StdPassword;
                    s.StdName = data.StdName;
                    s.StdLastname = data.StdLastname;
                    _db.Add(s);
                    _db.SaveChanges();
                    return View();
                }
                else
                {
                    ViewBag.Stutas = "StdCode ซ้ำ";
                    return View(data);
                }
            }
            else
            {
                ViewBag.Stutas = "error";
                return View(data);
            }
        }
        else
        {
            ViewBag.Stutas = "Password null";
            return View(data);
        }
    }
    
    public IActionResult Lab9_1()
    {
        var Student = (from s in _db.Labstudents select s).ToList();

        return View(Student);
    }
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
