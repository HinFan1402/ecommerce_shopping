using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce_shopping.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        public AccountController(SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager)
        {
            _userManage= userManager;
            _signInManager= signInManager;
        }
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl=returnUrl} );
        }
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không đúng!");
            }
            return View(loginVM);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public  async Task<IActionResult> Register(UserModel user)
        {
            if (ModelState.IsValid) 
            { 
                AppUserModel newUser = new AppUserModel(){UserName = user.UserName,Email = user.Email, };
                IdentityResult result = await  _userManage.CreateAsync(newUser, user.Password);
                if(result.Succeeded)
                {
                    await _signInManager.SignInAsync(newUser,isPersistent:false);
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("",error.Description);
                    }
                }
            }
            return View(user);
        }
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}
