using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data;
using StoreApp.Models;

namespace StoreApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly StoreContext _context;

    public HomeController(ILogger<HomeController> logger, StoreContext context)
    {
        _context = context;
        _logger = logger;
    }


    public IActionResult Index()
        {
            // Load products from the database
            var products = _context.Products.ToList();
            return View(products); // pass to Home/Index view
        }

   

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


