using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;

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
        // char A = 'a';
        string Name, Lastname, Sec, Year, Code;
        Name = "Naphat";
        Lastname = "Navahasdinkull";
        Sec = "L005";
        Year = "3Y";
        Code = "react";
        ViewBag.Name = Name;
        ViewBag.Lastname = Lastname;
        ViewBag.Sec = Sec;
        ViewBag.Year = Year;
        ViewBag.Code = Code;

        int a, b, c, d, e, f, g, h, i, j,sum ;
        a = 1;
        b = 2;
        c = 3;
        d = 4;
        e = 5;
        f = 6;
        g = 7;
        h = 8;
        i = 9;
        j = 10;
        sum = a + b + c + d + e + f + g + h + i + j;
        ViewBag.a = a;
        ViewBag.b = b;
        ViewBag.c = c;
        ViewBag.d = d;
        ViewBag.e = e;
       ViewBag.f = f;
        ViewBag.g = g;
        ViewBag.h = h;
        ViewBag.i = i;
        ViewBag.j = j;
        ViewBag.sum = sum;
        if (sum > 80)
        {
            ViewBag.result = "A";
        }
        if (sum > 76)
        {
            ViewBag.result = "B+";
        }
        if (sum > 70)
        {
            ViewBag.result = "B";
        }
        if (sum > 66)
        {
            ViewBag.result = "C+";
        }
        if (sum > 60)
        {
            ViewBag.result = "C";
        }
        if (sum > 56)
        {
            ViewBag.result = "D+";
        }
        if (sum > 50)
        {
            ViewBag.result = "D";
        }
        if (sum < 49)
        {
            ViewBag.result = "F";
        }


        // int x,y,sum;
        // x = 2;
        // y = 1;
        // sum = x + y;
        // ViewBag.x = x;
        // ViewBag.y = y;
        // ViewBag.sum = sum;
        // if (sum > 10)
        // {
        //     ViewBag.result = "True";
        // }
        // else if (sum > 0)
        // {
        //     ViewBag.result = "True2";
        // }
        // else
        // {
        //     ViewBag.result = "False";
        // }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
