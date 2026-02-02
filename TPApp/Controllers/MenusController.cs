using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string _mainDomain;

        public MenuController(AppDbContext context, IConfiguration config, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _config = config;
            _mainDomain = appSettings.Value.MainDomain;
        }

        [HttpGet]

        public IActionResult Detail(int menuId)
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var model = new MenuDetailViewModel
            {
                MenuId = menuId
            };

            // ===== BindInfo logic =====
            var p = _context.Menus
                .FirstOrDefault(x => x.MenuId == menuId && x.LanguageId == lang);

            if (p != null)
            {
                model.Title = p.Title;
                model.Description = p.Description;
            }

            var p1 = _context.Menus
                .Where(x =>
                    x.MenuPosition.Contains("4") &&
                    x.LanguageId == lang &&
                    x.ParentId == 1)
                .OrderBy(x => x.Sort)
                .Take(20)
                .Select(x => new MenuItemViewModel
                {
                    MenuId = x.MenuId,
                    Title = x.Title,
                    NavigateUrl = string.IsNullOrEmpty(x.NavigateUrl)
                        ? $"{_mainDomain}{x.QueryString}-{x.MenuId}.html"
                        : (x.NavigateUrl.Contains("http")
                            ? x.NavigateUrl
                            : $"{_mainDomain}{x.NavigateUrl}")
                })
                .ToList();

            model.Menus = p1;

            return View(model);
        }
    }
}
