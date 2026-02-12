using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;
using TPApp.ViewModel;

namespace TPApp.Controllers
{

    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string _mainDomain;

        // ===== GIỮ NGUYÊN LOGIC WEBFORMS =====
        private int SiteId => 1;
        private string DomainName => _mainDomain;

        public FeedbackController(AppDbContext context, IConfiguration config, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _config = config;
            _mainDomain = appSettings.Value.MainDomain;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new FeedbackCreateViewModel();

            // ===== LOAD USER FROM SESSION =====
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = _context.Users.FirstOrDefault(x => x.Id == userId.Value);
                if (user != null)
                {
                    vm.FullName = user.FullName ?? "";
                    vm.Email = user.Email ?? "";
                }
            }

            // ===== LOAD MENU ID = 74 =====
            var menu = _context.Menus.FirstOrDefault(x => x.MenuId == 74);
            if (menu != null)
            {
                vm.Title = menu.Title;
                vm.Description = menu.Description;
            }

            return View("Index", vm);
        }

        // =====================================================
        // POST: /lien-he-74.html
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FeedbackCreateViewModel vm)
        {
            var languageId = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            var lastPost = HttpContext.Session.GetString("PostedFeedback");
            var settingTime = _config.GetValue<int>("SettingTimeUpdatePageView");

            // ===== CHỐNG SPAM =====
            if (lastPost != null &&
                (DateTime.Now - DateTime.Parse(lastPost)).TotalSeconds < settingTime)
            {
                TempData["Alert"] = languageId == 1
                    ? "Ý kiến của bạn trước đó đang được xử lý. Vui lòng đợi ít phút trước khi gửi tiếp!"
                    : "Your comments are being processed. Please wait a few minutes.";

                return Redirect("/lien-he-74.html");
            }

            try
            {
                var feedback = new Feedback
                {
                    FullName = vm.FullName,
                    Email = vm.Email,
                    Address = vm.Address,
                    Phone = vm.Phone,
                    Content = vm.Content,
                    Created = DateTime.Now,
                    StatusId = 2,
                    SiteId = SiteId,
                    Domain = DomainName
                };

                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();

                // ===== SAVE POST TIME =====
                HttpContext.Session.SetString(
                    "PostedFeedback",
                    DateTime.Now.ToString("O")
                );

                TempData["Alert"] = languageId == 1
                    ? "Ý kiến của bạn đã được gửi. Cám ơn bạn đã đóng góp!"
                    : "Your comment has been submitted. Thanks for your contribution!";

                return Redirect("/lien-he-74.html");
            }
            catch
            {
                TempData["Alert"] = languageId == 1
                    ? "Lưu thất bại hãy kiểm tra lại"
                    : "Save failed check";

                return Redirect("/lien-he-74.html");
            }
        }
    }
}
