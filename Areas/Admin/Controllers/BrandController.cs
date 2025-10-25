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
    [Route("Admin/Brand")]

    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [Route("Index")]
        public async Task<IActionResult> Index(int pg=1)
        {
            List<BrandModel> brand = _dataContext.Brands.ToList();


            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = brand.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = brand.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        [Route("Edit")]
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
        [Route("Edit")]
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
        [Route("Create")]
        public IActionResult Create()
        {
            return View();

        }
        [HttpPost]
        [Route("Create")]
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
        [Route("Delete")]
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
