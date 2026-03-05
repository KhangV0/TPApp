using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.ViewComponents
{
    public class HeroBannerViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public HeroBannerViewComponent(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        /// <summary>
        /// Load hero banner slides from ImageAdver with Subject = 1, filtered by SiteId
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            var siteId = GetSiteId();

            var items = await _context.ImageAdvers
                .Where(x =>
                    x.StatusID == 3 &&
                    x.Subject == 1 &&
                    x.LanguageID == lang &&
                    (x.SiteId == null || x.SiteId == siteId))
                .OrderBy(x => x.Sort)
                .ToListAsync();

            return View(items);
        }
    }
}
