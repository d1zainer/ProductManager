using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// 
/// </summary>
/// <param name="logger"></param>
public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public IActionResult Index()
    {
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}