using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModel;
using ecommerce_shopping.Models.ViewModels;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Product")]
    [Authorize(Roles = "Admin")]

    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ProductController(DataContext datacontext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = datacontext;
            _webHostEnviroment = webHostEnvironment;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<ProductModel> Product = _dataContext.Products.Include(p => p.Brand)
        .Include(p => p.Category).ToList();


            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = Product.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;


            var data = Product.Skip(recSkip).Take(pager.PageSize).ToList();

            var viewModel = new TPaginateViewModel<ProductModel>
            {
                Items = data,
                Pager = pager
            };


            return View(viewModel);
        }
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Product already in database");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }
                else { Console.WriteLine("ImageUpload IS NULL"); }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Create product successfull";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model has something wrong!";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(product);
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id, ProductModel product)
        {
            ProductModel productEdit = await _dataContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                    string oldFilePath = Path.Combine(uploadsDir, productEdit.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                else
                {
                    product.Image = productEdit.Image;
                }

                _dataContext.Update(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update product successfull";
                return RedirectToAction("Index");

            }
            else
            { 
                TempData["error"] = "Model has something wrong!";
                return View(product); // 👈 Trả lại View để hiển thị lỗi và giữ data
            }
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(id);
            if (string.Equals(product.Image, "noimage.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "images");
                string filePath = Path.Combine(uploadsDir, product.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _dataContext.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete product successfull";
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("AddQuantity")]
        public async Task<IActionResult> AddQuantity(int Id)
        { 
            ViewBag.Product = _dataContext.Products.FirstOrDefault(p=>p.Id== Id);
            return View();
        }
        [HttpPost]
        [Route("UpdateQuantity")]
        public IActionResult UpdateQuantity(ProductQuantityModel productQuantityModel)
        {
            // Get the product to update
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound(); // Handle product not found scenario
            }
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.DateCreated = DateTime.Now;


            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("UpdateQuantity", "Product", new { Id = productQuantityModel.ProductId });
        }
    }

}