using Diplom_CRM.Models.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Diplom_CRM.Controllers;

[Authorize]
public class HomeController : Controller
{
    // GET: Home/Index
    public IActionResult Index()
    {
        return View();
    }

    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: Home/Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
