using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModels;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ecommerce_shopping.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _datacontext;
        private readonly UserManager<AppUserModel> _userManager; 
        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _userManager = userManager;
            _logger = logger;
            _datacontext = context;
        }

        public async Task<IActionResult> Index(ProductFilterViewModel filter)
        {
            // Set current controller and action for filter
            filter.CurrentController = "Home";
            filter.CurrentAction = "Index";

            // Get products with filters applied
            var query = _datacontext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // Apply filters
            query = query.ApplyFilters(filter);

            var products = query.ToList();

            // Pass filter to view for displaying current state
            ViewBag.Filter = filter;
            ViewBag.Slider = await _datacontext.Sliders.Where(s => s.Status == 1).ToListAsync();

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

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
        [HttpPost]
        public async Task<IActionResult> AddToWishList(int Id, WishListModel wishList)
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlistProduct = new WishListModel
            {
                ProductId = Id,
                User = user.Id
            };

            _datacontext.WishLists.Add(wishlistProduct);
            try
            {
                await _datacontext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to wishlisht Successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding to wishlist table.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddToCompare(int Id)
        {
            var user = await _userManager.GetUserAsync(User);

            var compareProduct = new CompareModel
            {
                ProductId = Id,
                User = user.Id
            };

            _datacontext.Compares.Add(compareProduct);
            try
            {
                await _datacontext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to compare Successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding to compare table.");
            }

        }
        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _datacontext.Compares
                                         join p in _datacontext.Products on c.ProductId equals p.Id
                                         join u in _datacontext.Users on c.User equals u.Id
                                         select new { User = u, Product = p, Compares = c })
                               .ToListAsync();

            return View(compare_product);
        }
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _datacontext.Compares.FindAsync(Id);

            _datacontext.Compares.Remove(compare);

            await _datacontext.SaveChangesAsync();
            TempData["success"] = "So sánh đã được xóa thành công";
            return RedirectToAction("Compare", "Home");
        }
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            WishListModel wishlist = await _datacontext.WishLists.FindAsync(Id);

            _datacontext.WishLists.Remove(wishlist);

            await _datacontext.SaveChangesAsync();
            TempData["success"] = "Yêu thích đã được xóa thành công";
            return RedirectToAction("Wishlist", "Home");
        }
        public async Task<IActionResult> WishList()
        {
            var wishlist_product = await (from w in _datacontext.WishLists
                                          join p in _datacontext.Products on w.ProductId equals p.Id
                                          select new { Product = p, WishLists = w })
                               .ToListAsync();

            return View(wishlist_product);
        }

        public IActionResult AccessDenied()
        {
            
            return View(); 
        }
        public async Task<IActionResult> Contact()
        {
            var contact =  await _datacontext.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }

    }
}