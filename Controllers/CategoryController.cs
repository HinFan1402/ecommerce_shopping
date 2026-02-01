using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _datacontext;
        public CategoryController(DataContext datacontext)
        {
            _datacontext = datacontext;
        }

        public async Task<IActionResult> Index(string Slug="")
        {
            CategoryModel category = _datacontext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();
            if (category == null) return RedirectToAction("Index");
            var productByCategory = _datacontext.Products.Where(p => p.CategoryId == category.Id);

            return View(await productByCategory.OrderByDescending(p => p.Id).ToListAsync());

        }
    }
}
