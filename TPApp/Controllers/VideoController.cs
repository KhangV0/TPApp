using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TPApp.Data;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class VideoController : Controller
    {
        private readonly AppDbContext _context;
        private const int PAGE_SIZE = 6;
        private const int PAGE_SHOW = 6;

        public VideoController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var model = new VideoIndexViewModel
            {
                CurrentPage = page
            };

            // ===== 1. Video nổi bật (MenuId = 70) =====
            var highlight = await _context.Contents
                .Where(q =>
                    q.StatusId == 3 &&
                    q.MenuId == 70 &&
                    q.PublishedDate <= DateTime.Now &&
                    (q.eEffectiveDate == null || q.eEffectiveDate >= DateTime.Now)
                )
                .OrderByDescending(q => q.PublishedDate)
                .FirstOrDefaultAsync();

            model.Highlight = highlight;

            if (highlight == null)
                return View(model);

            // ===== 2. Lấy submenu của 71 (CÁCH 2) =====
            var subMenuIds = await _context.Database
                .SqlQueryRaw<int>("EXEC uspSelectSubMenu @MenuId = {0}", 71)
                .ToListAsync();

            // ===== 3. Query base (GIỮ NGUYÊN LOGIC VB.NET) =====
            var baseQuery = _context.Contents.Where(q =>
                q.Id != highlight.Id &&
                (q.MenuId == 71 || subMenuIds.Contains(q.MenuId ?? 0)) &&
                q.StatusId == 3 &&
                q.MenuId != 47 &&
                q.MenuId != 48 &&
                q.PublishedDate <= DateTime.Now &&
                (q.eEffectiveDate == null || q.eEffectiveDate >= DateTime.Now)
            );

            // ===== 4. Count =====
            model.TotalRecords = await baseQuery.CountAsync();

            model.TotalPages = (int)Math.Ceiling(
                model.TotalRecords / (double)PAGE_SIZE
            );

            // ===== 5. Paging =====
            model.Videos = await baseQuery
                .OrderByDescending(q => q.PublishedDate)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();

            // ===== 6. Generate pager (GIỐNG Create_Pager) =====
            model.Pages = BuildPager(
                model.TotalPages,
                model.CurrentPage,
                PAGE_SHOW
            );

            return View(model);
        }

        // ===== Clone logic Create_Pager =====
        private List<int> BuildPager(int totalPage, int currentPage, int page2Show)
        {
            var pages = new HashSet<int>();

            if (currentPage <= page2Show)
            {
                for (int i = 1; i <= currentPage; i++)
                    pages.Add(i);
            }
            else
            {
                for (int i = currentPage - page2Show; i < currentPage; i++)
                    pages.Add(i);
            }

            if (currentPage + page2Show <= totalPage)
            {
                for (int i = currentPage; i <= currentPage + page2Show; i++)
                    pages.Add(i);
            }
            else
            {
                for (int i = currentPage; i <= totalPage; i++)
                    pages.Add(i);
            }

            return pages.OrderBy(x => x).ToList();
        }
    }
}
