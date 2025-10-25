using ecommerce_shopping.Models;
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
            List<OrderModel> Order = _dataContext.Orders.ToList();


            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = Order.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = Order.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        [Route("Edit")]
        public async Task<IActionResult> View(string orderCode)
        {
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