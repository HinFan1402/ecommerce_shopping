using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Role")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _userRoleManager;
        private readonly DataContext _dataContext;
        public RoleController(DataContext dataContext, RoleManager<IdentityRole> userRoleManager)
        {
            _dataContext = dataContext;
            _userRoleManager = userRoleManager;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Roles.OrderByDescending(b => b.Id).ToListAsync());
        }
        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            if (ModelState.IsValid)
            {
                if (!_userRoleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
                {
                    await _userRoleManager.CreateAsync(new IdentityRole(role.Name));
                }
                else
                {
                    TempData["error"] = "Role already exists!";
                    return View(role);
                }
                return RedirectToAction("Index");
            }
            TempData["error"] = "Model has something wrong!";
            return View(role);
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _userRoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            try
            {
                await _userRoleManager.DeleteAsync(role);
                TempData["success"] = "Role deleted successfully!";
            }
            catch
            {
                ModelState.AddModelError("", "Can't delete this role!");
            }
            return Redirect("Index");
        }
        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _userRoleManager.FindByIdAsync(id);
            return View(role);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole role)
        {
            if (ModelState.IsValid)
            {
                var existingRole = await _userRoleManager.FindByIdAsync(id);
                if (existingRole == null)
                {
                    return NotFound();
                }
                existingRole.Name = role.Name;
                var result = await _userRoleManager.UpdateAsync(existingRole);
                if (result.Succeeded)
                {
                    TempData["success"] = "Role updated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            TempData["error"] = "Model has something wrong!";
            return View(role);
        }

    }
}
