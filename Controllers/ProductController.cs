using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModels;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public ProductController(DataContext datacontext)
        {
            _dataContext = datacontext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int Id)
        {
            if (Id == 0) return RedirectToAction("Index");

            var productById = await _dataContext.Products
                .Include(p => p.Ratings) // lấy danh sách rating
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (productById == null) return NotFound();

            var relatedProduct = await _dataContext.Products
                .Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProduct;

            var ViewModel = new ProductDetailViewModel
            {
                Product = productById,
                Ratings = productById.Ratings, // danh sách đánh giá cũ
                NewRating = new RatingModel { ProductId = productById.Id } // đánh giá mới
            };

            return View(ViewModel);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .ToListAsync();

            ViewBag.Keyword = searchTerm;

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> CommentProduct([Bind(Prefix = "NewRating")] RatingModel rating)
        {
            var productIdFromForm = Request.Form["NewRating.ProductId"];
            Console.WriteLine($"ProductId from form: {productIdFromForm}");
            Console.WriteLine($"ProductId in model: {rating.ProductId}");
            // 1. Kiểm tra tham số quan trọng: ProductId (trước cả ModelState.IsValid)
            // Giả định ProductId là kiểu int và phải lớn hơn 0.
            if (rating == null )
            {
                // Chuyển hướng người dùng hoặc trả về lỗi nếu không có ID sản phẩm hợp lệ
                TempData["error"] = "Không tìm thấy thông tin sản phẩm để đánh giá.";
                return RedirectToAction("Index", "Home");
            }
            if (rating.ProductId ==0 )
            {
                // Chuyển hướng người dùng hoặc trả về lỗi nếu không có ID sản phẩm hợp lệ
                TempData["error"] = "Sai rồi";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                // Lấy lại product để render view nếu form không hợp lệ (BƯỚC NÀY ĐÃ AN TOÀN HƠN)
                var productById = await _dataContext.Products
                    .Include(p => p.Ratings)
                    .FirstOrDefaultAsync(p => p.Id == rating.ProductId);

                // 2. Bắt buộc phải kiểm tra kết quả truy vấn trước khi sử dụng
                if (productById == null)
                {
                    TempData["error"] = "Sản phẩm không tồn tại hoặc đã bị xóa.";
                    return Redirect(Request.Headers["Referer"].ToString());
                }

                var relatedProduct = await _dataContext.Products
                    // BƯỚC NÀY ĐÃ AN TOÀN vì productById đã được kiểm tra null
                    .Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id)
                    .Take(4)
                    .ToListAsync();

                ViewBag.RelatedProducts = relatedProduct;

                // Render lại trang chi tiết kèm dữ liệu cũ
                return View("Details", new ProductDetailViewModel
                {
                    Product = productById,
                    Ratings = productById.Ratings,
                    NewRating = rating
                });
            }

            // ... (Phần logic lưu đánh giá khi ModelState.IsValid là true)
            _dataContext.Ratings.Add(rating);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đánh giá thành công!";
            return Redirect(Request.Headers["Referer"].ToString());
        }



    }
}

