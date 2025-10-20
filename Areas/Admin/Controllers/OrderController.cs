using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }
        public async Task<IActionResult> View(string orderCode)
        {
            var detailOrder= await _dataContext.OrderDetails.Include(od=>od.Product).Where(od => od.OrderCode == orderCode).ToListAsync();
            return View(detailOrder);
        }
    }
}
