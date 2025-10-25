using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ecommerce_shopping.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _datacontext; 
        public HomeController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
            _datacontext = context;
        }

        public IActionResult Index()
        {
            var products= _datacontext.Products.ToList();
            var activeSliders =_datacontext.Sliders.Where(s => s.Status == 1).ToList();
            ViewBag.ActiveSliders = activeSliders;
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            else
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AccessDenied()
        {
            
            return View(); 
        }
        public IActionResult Contact()
        {
            return View();
        }
    }
}