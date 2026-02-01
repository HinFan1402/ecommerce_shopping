using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModels;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }

            // Count on DB side
            int recsCount = await _dataContext.Orders.CountAsync();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            // Load only the page of data we need, order by a deterministic column
            var data = await _dataContext.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreateDate)
                .Skip(recSkip)
                .Take(pager.PageSize)
                .ToListAsync();

            var viewModel = new TPaginateViewModel<OrderModel>
            {
                Items = data,
                Pager = pager
            };

            return View(viewModel);
        }
        [Route("Edit")]
        public async Task<IActionResult> View(string orderCode)
        {
            // Use async read and AsNoTracking for read-only values
            ViewBag.Order = await _dataContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            var detailOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == orderCode).ToListAsync();
            return View(detailOrder);
        }
        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string orderCode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
            {
                return NotFound();
            }

            order.status = 1;

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception)
            {


                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
    }

}