using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce_shopping.Controllers
{
    public class ProductController: Controller
    {
        private readonly DataContext _datacontext;
        public ProductController(DataContext datacontext)
        {
            _datacontext = datacontext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int Id)
        {
            if (Id == null) return RedirectToAction("Index");

            var productById = _datacontext.Products.Where(p => p.Id == Id).FirstOrDefault();
            return View(productById);
        }
    }
}
