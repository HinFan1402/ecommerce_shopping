using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModel;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Mvc;
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
            List<CartItemModel> cartItem = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel cartItemVM = new()
            {
                CartItems = cartItem,
                GrandTotal = cartItem.Sum(x => x.Quantity * x.Price)

            };
            return View(cartItemVM);
        }
        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Add(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem == null)
            {
                cart.Add(new CartItemModel(product));
            }
            else
                cartItem.Quantity += 1;
            HttpContext.Session.SetJson("Cart", cart);
            TempData["success"] = "Add item succesfully!";
            return Redirect(Request.Headers["Referer"].ToString()); // "Referer" is correct as per HTTP spec, not a spelling error in code context
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
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem.Quantity >= 1)
            {
                ++cartItem.Quantity;
            }
            HttpContext.Session.SetJson("Cart", cart);
            TempData["success"] = "Increase item succesfully!";
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

    }
}
