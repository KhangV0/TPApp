using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.ViewComponents
{
    public class ManLuoiDoiTacVm
    {
        public List<ImageAdver> SanGiaoDich { get; set; } = new();  // Subject = 2
        public List<ImageAdver> DonViTuVan { get; set; } = new();   // Subject = 3
        public List<ImageAdver> DoiTac { get; set; } = new();       // Subject = 5
    }

    public class ManLuoiDoiTacViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ManLuoiDoiTacViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var allItems = await _context.ImageAdvers
                .AsNoTracking()
                .Where(x =>
                    x.StatusID == 3 &&
                    x.LanguageID == lang &&
                    (x.Subject == 2 || x.Subject == 3 || x.Subject == 5))
                .OrderBy(x => x.Sort)
                .ThenByDescending(x => x.Created)
                .ToListAsync();

            var vm = new ManLuoiDoiTacVm
            {
                SanGiaoDich = allItems.Where(x => x.Subject == 2).ToList(),
                DonViTuVan  = allItems.Where(x => x.Subject == 3).ToList(),
                DoiTac      = allItems.Where(x => x.Subject == 5).ToList()
            };

            return View(vm);
        }
    }
}
