using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Entities;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

                return RedirectToAction("Index", "Home");
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
                        return RedirectToAction("Index", "Home");
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
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
