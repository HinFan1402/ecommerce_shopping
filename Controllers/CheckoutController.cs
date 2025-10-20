using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ecommerce_shopping.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        public CheckoutController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                TempData["error"] = "Không xác định được tài khoản";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var orderCode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.OrderCode = orderCode;
                orderItem.UserName = userEmail;
                orderItem.status = 1;
                orderItem.CreateDate = DateTime.Now;

                _dataContext.Orders.Add(orderItem);
                await _dataContext.SaveChangesAsync();
         
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderdetails = new OrderDetailModel();
                    orderdetails.OrderCode = orderCode;
                    orderdetails.UserName = userEmail;
                    orderdetails.ProductId = cart.ProductId;
                    orderdetails.Price = cart.Price;
                    orderdetails.Quantity = cart.Quantity;
                   
                    _dataContext.Add(orderdetails);
                    await _dataContext.SaveChangesAsync();
                }
                HttpContext.Session.Remove("Cart");
                TempData["success"]="Tạo đơn hàng thành công, đang chờ duyệt";
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
