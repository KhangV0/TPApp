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


        [HttpGet]
        [Route("{queryString:regex(^tin-su-kien|hoi-thao-trinh-dien-cong-nghe$)}-{menuId:int}.html")]
        public async Task<IActionResult> Category(int menuId, int page = 1)
        {
            const int pageSize = 10;

            var langId = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var menu = await GetMenuAsync(menuId);
            if (menu == null)
                return Redirect($"{MainDomain}Errors/404.aspx");

            var subMenuIds = GetSubMenuIds(menuId);

            var (items, total) = await GetNewsByMenuAsync(
                menuId,
                subMenuIds,
                langId,
                page,
                pageSize
            );

            var pager = BuildPager(total, page, pageSize, 10);

            var vm = new NewsCategoryVm
            {
                MenuId = menuId,
                CategoryTitle = menu.Title,
                Items = items,
                Pager = pager
            };

            ViewData["Title"] = menu.Title;

            return View(vm);
        }


        private async Task<Menu?> GetMenuAsync(int menuId)
        {
            return await _context.Menus
                .FirstOrDefaultAsync(x => x.MenuId == menuId);
        }


        private List<int> GetSubMenuIds(int menuId)
        {
            return _context
                .UspSelectSubMenu(menuId)
                .Select(x => x)
                .ToList();
        }


        private async Task<(List<NewsItemVm> Items, int Total)>
    GetNewsByMenuAsync(
        int menuId,
        List<int> subMenus,
        int langId,
        int page,
        int pageSize)
        {
            var now = DateTime.Now;

            var query = _context.Contents
                .Where(q =>
                    (q.MenuId == menuId || subMenus.Contains(q.MenuId ?? 0)) &&
                    q.LanguageId == langId &&
                    q.StatusId == 3 &&
                    q.IsReport == false &&
                    q.PublishedDate <= now &&
                    (q.eEffectiveDate == null || q.eEffectiveDate >= now)
                );

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(q => q.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new NewsItemVm
                {
                    Id = q.Id,
                    MenuId = q.MenuId,
                    Title = q.Title,
                    QueryString = q.QueryString,
                    Image = q.Image,
                    Description = q.Description,
                    PublishedDate = q.PublishedDate
                })
                .ToListAsync();

            return (items, total);
        }


        private PagerVm BuildPager(
            int totalRecord,
            int currentPage,
            int pageSize,
            int pageToShow)
        {
            var totalPage = (int)Math.Ceiling(totalRecord / (double)pageSize);

            var pages = new HashSet<int>();

            for (int i = Math.Max(1, currentPage - pageToShow);
                 i <= Math.Min(totalPage, currentPage + pageToShow);
                 i++)
            {
                pages.Add(i);
            }

            return new PagerVm
            {
                TotalRecord = totalRecord,
                TotalPage = totalPage,
                CurrentPage = currentPage,
                Pages = pages.OrderBy(x => x).ToList()
            };
        }

    }
}
