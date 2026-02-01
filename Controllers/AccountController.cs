using ecommerce_shopping.Areas.Admin.Repository;
using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModels;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ecommerce_shopping.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        public AccountController(SignInManager<AppUserModel> signInManager,
            UserManager<AppUserModel> userManager, DataContext context,IEmailSender emailSender)
        {
            _dataContext = context;
            _userManage = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email not found";
                return RedirectToAction("ForgotPass", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                //update token to user
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();
                var receiver = checkMail.Email;
                var subject = "Thay đổi mật khẩu! " + checkMail.Email;
                var callbackUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("ResetPass", "Account", new { email = checkMail.Email, token = token })}";
                var message = $@"
                    <p>Chúng tôi nhận được yêu cầu reset mật khẩu từ tài khoản của bạn.</p>
                    <p>Truy cập link bên dưới để reset mật khẩu của bạn:</p>
                    <a href='{callbackUrl}'>Reset Password</a>
                    <br><br>
                    <p>Nếu bạn không yêu cầu thay đổi, bỏ qua email này.</p>";
                try
                {
                    await _emailSender.SenderEmailAsync(receiver, subject, message);
                }
                catch(Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }


            TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
            return RedirectToAction("ForgotPass", "Account");
        }
        public IActionResult ForgotPass()
        {
            return View();
        }
        public async Task<IActionResult> Index()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Account/History" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        public async Task<IActionResult> UpdateInfoAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userById = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if( userById == null)
            {
                return NotFound();
            }
            else
            {
                _dataContext.Update(userById);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin tài khoản thành công";
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> ResetPass(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                //update user with new password and token
                string newtoken = Guid.NewGuid().ToString();
                // Hash the new password
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManage.UpdateAsync(checkuser);
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Account/History" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _dataContext.Orders.Where(o => o.UserName == userEmail).ToListAsync();
            return View(orders);
        }
        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                order.status = 2;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                return BadRequest("An error occurred while canceling the order." + ex.Message);
            }


            return RedirectToAction("History", "Account");
        }
        [HttpGet]
        //---------------------------Login Normal--------------------------------
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
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
        public async Task<IActionResult> Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại
                var existingUser = await _userManage.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký");
                    TempData["errors"] = "Email này đã tồn tại!";
                    return View(user);
                }

                // Tạo mã OTP 6 chữ số
                string otpCode = GenerateOTP();

                // Lưu OTP vào TempData (hoặc Session/Cache)
                TempData["OTP"] = otpCode;
                TempData["UserName"] = user.UserName;
                TempData["Email"] = user.Email;
                TempData["Password"] = user.Password;
                TempData["OTPExpiry"] = DateTime.Now.AddMinutes(5); // OTP hết hạn sau 5 phút

                // Gửi OTP qua email
                try
                {
                    await _emailSender.SenderEmailAsync(
                        user.Email,
                        "Mã xác thực đăng ký tài khoản",
                        $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #337ab7;'>Xác thực đăng ký tài khoản</h2>
                            <p>Xin chào <strong>{user.UserName}</strong>,</p>
                            <p>Mã OTP của bạn là:</p>
                            <div style='background: #f0ad4e; padding: 15px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #fff; border-radius: 5px;'>
                                {otpCode}
                            </div>
                            <p style='color: #d9534f; margin-top: 15px;'><strong>Lưu ý:</strong> Mã này có hiệu lực trong 5 phút.</p>
                            <p>Nếu bạn không thực hiện đăng ký, vui lòng bỏ qua email này.</p>
                            </div>
                        "
                    );

                    return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Không thể gửi email. Vui lòng thử lại." });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otpCode)
        {
            string savedOTP = TempData["OTP"]?.ToString();
            DateTime? otpExpiry = TempData["OTPExpiry"] as DateTime?;
            string userName = TempData["UserName"]?.ToString();
            string email = TempData["Email"]?.ToString();
            string password = TempData["Password"]?.ToString();

            // Giữ lại TempData để có thể thử lại
            TempData.Keep();

            if (string.IsNullOrEmpty(savedOTP) || string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Phiên làm việc đã hết hạn. Vui lòng đăng ký lại." });
            }

            if (otpExpiry.HasValue && DateTime.Now > otpExpiry.Value)
            {
                TempData.Clear();
                return Json(new { success = false, message = "Mã OTP đã hết hạn. Vui lòng đăng ký lại." });
            }

            if (otpCode != savedOTP)
            {
                return Json(new { success = false, message = "Mã OTP không chính xác. Vui lòng thử lại." });
            }

            // OTP đúng, tiến hành tạo tài khoản
            AppUserModel newUser = new AppUserModel()
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true // Đánh dấu email đã xác thực
            };

            IdentityResult result = await _userManage.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                TempData.Clear();
                return Json(new { success = true, message = "Đăng ký thành công!", redirectUrl = Url.Action("Login", "Home") });
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errors });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResendOTP()
        {
            string email = TempData["Email"]?.ToString();
            string userName = TempData["UserName"]?.ToString();

            TempData.Keep();

            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Phiên làm việc đã hết hạn." });
            }

            // Tạo mã OTP mới
            string otpCode = GenerateOTP();
            TempData["OTP"] = otpCode;
            TempData["OTPExpiry"] = DateTime.Now.AddMinutes(5);
            TempData.Keep();

            try
            {
                await _emailSender.SenderEmailAsync(
                    email,
                    "Mã xác thực đăng ký tài khoản",
                    $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #337ab7;'>Mã OTP mới</h2>
                            <p>Xin chào <strong>{userName}</strong>,</p>
                            <p>Mã OTP mới của bạn là:</p>
                            <div style='background: #f0ad4e; padding: 15px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #fff; border-radius: 5px;'>
                                {otpCode}
                            </div>
                            <p style='color: #d9534f; margin-top: 15px;'><strong>Lưu ý:</strong> Mã này có hiệu lực trong 5 phút.</p>
                        </div>
                    "
                );

                return Json(new { success = true, message = "Mã OTP mới đã được gửi!" });
            }
            catch
            {
                return Json(new { success = false, message = "Không thể gửi email." });
            }
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return Redirect(returnUrl);
        }
        /// 
        /// /---------------------------------Login Google------------------------------------------------------------------------------
     
        public async Task LoginByGoogle()
        {
            // Use Google authentication scheme for challenge
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        public async Task<IActionResult>   GoogleResponse()
        {
            // Authenticate using Google scheme
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                //Nếu xác thực ko thành công quay về trang Login
                return RedirectToAction("Login");
            }

            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string emailName = email.Split('@')[0];
            //return Json(claims);
            // Check user có tồn tại không
            var existingUser = await _userManage.FindByEmailAsync(email);

            if (existingUser == null)
            {
                //nếu user ko tồn tại trong db thì tạo user mới với password hashed mặc định 1-9
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "123456789");
                //username thay khoảng cách bằng dấu "-" và chữ thường hết
                var newUser = new AppUserModel { UserName = emailName, Email = email };
                newUser.PasswordHash = hashedPassword; // Set the hashed password cho user

                var createUserResult = await _userManage.CreateAsync(newUser);
                if (!createUserResult.Succeeded)
                {
                    TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
                    return RedirectToAction("Login", "Account"); // Trả về trang đăng ký nếu fail

                }
                else
                {
                    // Nếu user tạo user thành công thì đăng nhập luôn 
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["success"] = "Đăng ký tài khoản thành công.";
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                //Còn user đã tồn tại thì đăng nhập luôn với existingUser
                TempData["success"] = "Đăng nhập thành công.";
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            

        }
    }
}
