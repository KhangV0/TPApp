using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.ViewModel;
using System.Globalization;

namespace TPApp.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private const string MainDomain = "https://localhost:7232/";
        public NewsController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [Route("{menuId:int}/{queryString}-{id:long}.html")]

        [HttpGet]
        public async Task<IActionResult> Detail(
            int menuId,
            string queryString,
            long id)
        {

            var p = await _context.Contents
                .Where(x => x.Id == id && x.StatusId == 3)
                .OrderByDescending(x => x.PublishedDate)
                .FirstOrDefaultAsync();

            if (p == null)
                return Redirect($"{MainDomain}Errors/404.aspx");

            // === CHECK ROUTE (GIỐNG HỆT WEBFORMS) ===
            if (menuId != p.MenuId || queryString != p.QueryString)
                return Redirect($"{MainDomain}Errors/404.aspx");

            // === META ===
            ViewData["Title"] = p.Title;
            ViewData["MetaDescription"] = p.Description;

            // === DATE FORMAT ===
            string? publishedDate = null;
            if (p.PublishedDate.HasValue)
            {
                publishedDate = p.PublishedDate.Value
                    .ToString("dddd, d/M/yyyy, HH:mm",
                        new CultureInfo("vi-VN")) + " (GMT+7)";
            }

            // === IMAGE (TypeId = 4) ===
            var images = new List<Album>();
            if (p.TypeId == 4)
            {
                images = await _context.Albums
                    .Where(x => x.ContensID == p.Id)
                    .ToListAsync();
            }

            // === UPDATE VIEW ===
            if (p.Viewed == null)
            {
                p.Viewed = 168;
                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionView = HttpContext.Session.GetString("ViewNews");
                var updateTime = _config.GetValue<int>("SettingTimeUpdatePageView");

                if (sessionView == null ||
                    (DateTime.Now - DateTime.Parse(sessionView)).TotalSeconds >= updateTime)
                {
                    p.Viewed += 1;
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("ViewNews", DateTime.Now.ToString());
                }
            }

            // === RELATED NEWS ===
            var langId = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var subMenus = _context
                .UspSelectSubMenu(p.MenuId ?? 0)
                .ToList();

            var related = await _context.Contents
                .Where(x =>
                    x.Id != id &&
                    x.StatusId == 3 &&
                    (x.MenuId == p.MenuId || subMenus.Contains(x.MenuId ?? 0)) &&
                    x.LanguageId == langId)
                .OrderByDescending(x => x.PublishedDate)
                .Take(5)
                .Select(x => new RelatedNewsVm
                {
                    Id = x.Id,
                    Title = x.Title,
                    QueryString = x.QueryString,
                    MenuId = x.MenuId,
                    PublishedDate = x.PublishedDate
                })
                .ToListAsync();

            // === VIEWMODEL ===
            var vm = new NewsDetailVm
            {
                Id = p.Id,
                MenuId = p.MenuId ?? menuId,
                Title = p.Title,
                Description = p.Description,
                Content = p.Contents,
                Author = (p.TypeId == 5 && p.MenuId == 46) ? "" : p.Author,
                PublishedDateText = publishedDate,
                Images = images,
                Related = related
            };

            return View(vm);
        }
    }
}
