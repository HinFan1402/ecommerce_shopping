using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModel;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace ecommerce_shopping.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        public CartController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IActionResult Index()
        {
            var coupon_code = Request.Cookies["CouponTitle"];
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
                ShippingCost = 0
            };

            return View(cartVM);
        }
        [HttpPost]
        public IActionResult CalculateShipping([FromBody] string province)
        {
            decimal shippingCost = 0;

            // Danh sách các cách gọi HCM
            var hcmNames = new List<string>
            {
                "Thành phố Hồ Chí Minh",
                "Hồ Chí Minh",
                "TP. Hồ Chí Minh",
                "TP.HCM",
                "HCM"
            };

            // Kiểm tra nếu thuộc HCM
            if (hcmNames.Any(p => province.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                shippingCost = 25000; // 25k cho nội thành HCM
            }
            else
            {
                shippingCost = 75000; // 75k cho ngoại tỉnh
            }

            return Json(new { shippingCost = shippingCost });
        }
        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Add(int Id)
        {
            var product = await _dataContext.Products.FindAsync(Id);
            if (product == null)
                return NotFound(new { success = false, message = "Product not found" });

            // Lấy giỏ hàng trong session
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItem == null)
                cart.Add(new CartItemModel(product));
            else
                cartItem.Quantity += 1;

            HttpContext.Session.SetJson("Cart", cart);

            // ✅ Trả về JSON để Ajax xử lý
            return Ok(new { success = true, message = "Add item successfully!" });
        }

        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem.Quantity > 1)
            {
                --cartItem.Quantity;
            }
            else
            {
                cart.RemoveAll(p => p.ProductId == Id);
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["success"] = "Decrease item succesfully!";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Increase(int Id)
        {
            var product = _dataContext.Products.First(p => p.Id == Id);
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem.Quantity >= 1 && cartItem.Quantity >= product.Quantity)
            {
                cartItem.Quantity = product.Quantity;
                TempData["error"] = "Quantity is max!";
            }
            else
            {
                ++cartItem.Quantity;
                TempData["success"] = "Increase item succesfully!";
            }
            HttpContext.Session.SetJson("Cart", cart);
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Remove(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            cart.RemoveAll(p => p.ProductId == Id);

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["success"] = "Remove item succesfully!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> GetCoupon(string coupon_value)
        {
            try
            {
                var coupon = await _dataContext.Coupons.FirstOrDefaultAsync(x => x.CouponCode == coupon_value);

                if (coupon == null)
                    return Ok(new { success = false, message = "Coupon not existed" });

                if (coupon.DateExpired < DateTime.Now)
                    return Ok(new { success = false, message = "Coupon has expired" });

                return Ok(new
                {
                    success = true,
                    message = "Coupon applied successfully",
                    priceCoupon = coupon.PriceCoupon,
                    title = $"{coupon.CouponCode} | {coupon.Description}"
                });
            }
            catch(Exception ex)
            {

            
                Console.WriteLine("🔥 Lỗi server khi GetCoupon: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        


    }


}

