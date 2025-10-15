using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IActionResult> Index(string slug = "")
        {
            BrandModel brand = _dataContext.Brands.Where(b => b.Slug == slug).FirstOrDefault();
            if (brand == null) return RedirectToAction("Index");
            var productByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
            return View(await productByBrand.OrderByDescending(p=> p.BrandId ==brand.Id).ToListAsync());
        }

    }
}
