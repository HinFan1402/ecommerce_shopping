using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _userRoleManager;

        public UserController(DataContext dataContext,UserManager<AppUserModel> userManager, RoleManager<IdentityRole> userRoleManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _userRoleManager = userRoleManager;

        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var userWithRole = await (from u in _dataContext.Users
                                      join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                      join r in _dataContext.Roles on ur.RoleId equals r.Id
                                      select new {User =u ,RoleName= r.Name}).ToListAsync();
            return View(userWithRole);
        }
        [Route("Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _userRoleManager.Roles.ToArrayAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PassWordNoHash);
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email);
                    var userId = createUser.Id;
                    var role = _userRoleManager.FindByIdAsync(user.RoleId);

                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {

                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    TempData["error"] = "Model has something wrong!";
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }
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
        }
        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var roles = await _userRoleManager.Roles.ToArrayAsync();
            var user = await _userManager.FindByIdAsync(id);
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
            }

            var roles = await _userRoleManager.Roles.ToArrayAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            TempData["error"] = "Model validation failed.";
            var error = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", error);
            return View(existingUser);
        }

        private void AddIdentityErrors(IdentityResult updateUserResult)
        {
           foreach(var error in updateUserResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [Route("Delete")]
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded) { return View("Error"); }
            TempData["success"] = "User đã xóa!";
            return RedirectToAction("Index");
        }

    }

}
