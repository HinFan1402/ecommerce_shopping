using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    [Route("Admin/Category")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [Route("Index")]
        public async Task<IActionResult> Index(int pg=1)
        {
            List<CategoryModel> Category = _dataContext.Categories.ToList();


            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = Category.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = Category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id, CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Category successfull";
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
            return View(category);
        }
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();

        }
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(int Id, CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Category successfull";
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
            return View(category);
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(id);
            _dataContext.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete product successfull";
            return RedirectToAction("Index");
        }
    }
}