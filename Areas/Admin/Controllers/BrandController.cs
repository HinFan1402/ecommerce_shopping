using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]

    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Brands.OrderByDescending(b => b.Id).ToListAsync());
        }
    
        public async Task<IActionResult> Edit(int id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int Id, BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Brand successfull";
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
            return View(brand);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> Create(int Id, BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Brand successfull";
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
            return View(brand);
        }
        public async Task<IActionResult> Delete(int id)
        {
            BrandModel brands = await _dataContext.Brands.FindAsync(id);
            _dataContext.Remove(brands);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete product successfull";
            return RedirectToAction("Index");
        }
    }
}
