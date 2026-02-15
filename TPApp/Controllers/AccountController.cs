using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Entities;
using TPApp.ViewModel;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccountService _accountService;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /dang-ky.html
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, [FromServices] TPApp.Data.AppDbContext context)
        {
            if (ModelState.IsValid)
            {
                // Manual check because Normalized columns are ignored
                if (context.Users.Any(u => u.UserName == model.UserName))
                {
                     ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                     return View(model);
                }
                // Check Email if needed. IdentityUser allows duplicate emails by default if configured so, 
                // but usually we want unique. Note: NormalizedEmail is ignored so checking Email directly.
                if (context.Users.Any(u => u.Email == model.Email))
                {
                     ModelState.AddModelError("Email", "Email đã tồn tại.");
                     return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName, 
                    Email = model.Email,
                    FullName = model.FullName,
                    Created = DateTime.Now,
                    IsActivated = true,
                    Domain = Request.Host.Host,
                    // PhoneNumber maps to Mobile column automatically
                    PhoneNumber = model.PhoneNumber 
                };

                // Manual Password Hashing
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);

                // Add to Context directly to bypass Identity Validators that might rely on Normalized columns
                context.Users.Add(user);
                await context.SaveChangesAsync();
                
                // Sign In
                await _signInManager.SignInAsync(user, isPersistent: false);
                    
                // Set Session for legacy support
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.UserName);

                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /dang-nhap.html
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, [FromServices] TPApp.Data.AppDbContext context, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Manual lookup because Normalized columns are ignored
                var user = context.Users.FirstOrDefault(u => u.UserName == model.UserName);
                
                if (user != null)
                {
                    // Verify password
                    var passwordVerification = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                    if (passwordVerification == PasswordVerificationResult.Success)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                        user.LastLogin = DateTime.Now;
                        await _userManager.UpdateAsync(user);

                        // Set Session for legacy support
                        HttpContext.Session.SetInt32("UserId", user.Id);
                        HttpContext.Session.SetString("Username", user.UserName);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // POST: /Account/LoginAjax - AJAX Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAjax(LoginViewModel model, [FromServices] TPApp.Data.AppDbContext context, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            // Manual lookup because Normalized columns are ignored
            var user = context.Users.FirstOrDefault(u => u.UserName == model.UserName);
            
            if (user != null)
            {
                // Verify password
                var passwordVerification = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                if (passwordVerification == PasswordVerificationResult.Success)
                {
                    await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                    user.LastLogin = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    // Set Session for legacy support
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.UserName);

                    var redirectUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
                        ? returnUrl 
                        : Url.Action("Index", "Dashboard");

                    return Json(new { success = true, redirectUrl });
                }
            }
            
            return Json(new { success = false, errors = new[] { "Tên đăng nhập hoặc mật khẩu không chính xác." } });
        }

        // POST: /Account/RegisterAjax - AJAX Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAjax(RegisterViewModel model, [FromServices] TPApp.Data.AppDbContext context)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            // Manual check because Normalized columns are ignored
            if (context.Users.Any(u => u.UserName == model.UserName))
            {
                return Json(new { success = false, errors = new[] { "Tên đăng nhập đã tồn tại." } });
            }

            if (context.Users.Any(u => u.Email == model.Email))
            {
                return Json(new { success = false, errors = new[] { "Email đã tồn tại." } });
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName, 
                Email = model.Email,
                FullName = model.FullName,
                Created = DateTime.Now,
                IsActivated = true,
                Domain = Request.Host.Host,
                PhoneNumber = model.PhoneNumber 
            };

            // Manual Password Hashing
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);

            // Add to Context directly
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            // Sign In
            await _signInManager.SignInAsync(user, isPersistent: false);
                
            // Set Session for legacy support
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.UserName);

            var redirectUrl = Url.Action("Index", "Dashboard");
            return Json(new { success = true, redirectUrl });
        }


        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var model = await _accountService.GetProfileAsync(userId);
            
            if (model == null)
                return NotFound();
            
            return View(model);
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVm model, IFormFile? avatar)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(_userManager.GetUserId(User)!);

            try
            {
                // Upload avatar if provided
                if (avatar != null)
                {
                    var avatarPath = await _accountService.UploadAvatarAsync(userId, avatar);
                    model.AvatarUrl = avatarPath;
                }

                // Update profile
                var success = await _accountService.UpdateProfileAsync(userId, model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Profile));
                }
                
                ModelState.AddModelError("", "Không thể cập nhật thông tin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        // GET: /Account/ChangePassword
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(_userManager.GetUserId(User)!);

            try
            {
                var success = await _accountService.ChangePasswordAsync(userId, model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction(nameof(Profile));
                }
                
                ModelState.AddModelError("", "Không thể đổi mật khẩu");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear(); // Clear legacy session
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
