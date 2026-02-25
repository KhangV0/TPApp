using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Areas.Cms.Controllers
{
    // ── DTOs ──
    public class ImageAdverListItem
    {
        public int ID { get; set; }
        public string? Title { get; set; }
        public string? SRC { get; set; }
        public string? URL { get; set; }
        public int? Subject { get; set; }
        public int? StatusID { get; set; }
        public string? StatusTitle { get; set; }
        public int? Sort { get; set; }
        public string? Creator { get; set; }
        public DateTime? Created { get; set; }
    }

    public class ImageAdverFormVm
    {
        public int ID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? SRC { get; set; }
        public string? URL { get; set; }
        public int? Subject { get; set; }
        public int? StatusID { get; set; }
        public int? Sort { get; set; }
        public int? ParentId { get; set; }
        public int LanguageID { get; set; } = 1;
        public string Domain { get; set; } = string.Empty;
        public int? SiteId { get; set; }
        public string? Creator { get; set; }
        public DateTime? Created { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize]
    public class ImageAdverAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ImageAdverAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private string GetDomain() =>
            _configuration["AppSettings:Domain"] ?? "";

        // ── INDEX ──
        public async Task<IActionResult> Index(
            string? keyword, int? statusId, int? subject,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var siteId = GetSiteId();

            var query = _context.ImageAdvers.AsNoTracking()
                .Where(a => a.LanguageID == 1 && (a.SiteId == null || a.SiteId == siteId));

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(a => a.Title != null && a.Title.Contains(keyword));
            if (statusId.HasValue)
                query = query.Where(a => a.StatusID == statusId.Value);
            if (subject.HasValue)
                query = query.Where(a => a.Subject == subject.Value);

            // Sort
            query = sortBy?.ToLower() switch
            {
                "title" => sortDir == "desc" ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                "sort" => sortDir == "desc" ? query.OrderByDescending(a => a.Sort) : query.OrderBy(a => a.Sort),
                "status" => sortDir == "desc" ? query.OrderByDescending(a => a.StatusID) : query.OrderBy(a => a.StatusID),
                "created" => sortDir == "desc" ? query.OrderByDescending(a => a.Created) : query.OrderBy(a => a.Created),
                _ => query.OrderBy(a => a.Sort).ThenByDescending(a => a.Created)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            // Statuses
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();
            var statusDict = statuses.ToDictionary(s => s.StatusId, s => s.Title);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ImageAdverListItem
                {
                    ID = a.ID,
                    Title = a.Title,
                    SRC = a.SRC,
                    URL = a.URL,
                    Subject = a.Subject,
                    StatusID = a.StatusID,
                    Sort = a.Sort,
                    Creator = a.Creator,
                    Created = a.Created
                })
                .ToListAsync();

            // Map status titles
            foreach (var item in items)
            {
                if (item.StatusID.HasValue && statusDict.TryGetValue(item.StatusID.Value, out var sTitle))
                    item.StatusTitle = sTitle;
            }

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.Subject = subject;
            ViewBag.Statuses = statuses;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        // ── CREATE GET ──
        public async Task<IActionResult> Create()
        {
            var vm = new ImageAdverFormVm
            {
                LanguageID = 1,
                Domain = GetDomain(),
                SiteId = GetSiteId(),
                StatusID = 1,
                Sort = 0
            };
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ── CREATE POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImageAdverFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = MapToEntity(vm);
            entity.Created = DateTime.Now;
            entity.Creator = User.Identity?.Name;

            _context.ImageAdvers.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo quảng cáo thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ──
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.ImageAdvers.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToVm(entity);
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ── EDIT POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ImageAdverFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = await _context.ImageAdvers.FindAsync(vm.ID);
            if (entity == null) return NotFound();

            entity.Title = vm.Title;
            entity.Description = vm.Description;
            entity.SRC = vm.SRC;
            entity.URL = vm.URL;
            entity.Subject = vm.Subject;
            entity.StatusID = vm.StatusID;
            entity.Sort = vm.Sort;
            entity.ParentId = vm.ParentId;
            entity.SiteId = vm.SiteId;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật quảng cáo thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.ImageAdvers.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy quảng cáo." });

            if (entity.StatusID == 3)
                return Json(new { success = false, message = "Không thể xóa quảng cáo đang xuất bản." });

            _context.ImageAdvers.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa quảng cáo #" + id });
        }

        // ── HELPERS ──
        private async Task LoadFormSelectListsAsync()
        {
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "Title");
        }

        private ImageAdver MapToEntity(ImageAdverFormVm vm) => new()
        {
            Title = vm.Title,
            Description = vm.Description,
            SRC = vm.SRC,
            URL = vm.URL,
            Subject = vm.Subject,
            StatusID = vm.StatusID,
            Sort = vm.Sort,
            ParentId = vm.ParentId,
            LanguageID = vm.LanguageID,
            Domain = vm.Domain,
            SiteId = vm.SiteId
        };

        private ImageAdverFormVm MapToVm(ImageAdver e) => new()
        {
            ID = e.ID,
            Title = e.Title,
            Description = e.Description,
            SRC = e.SRC,
            URL = e.URL,
            Subject = e.Subject,
            StatusID = e.StatusID,
            Sort = e.Sort,
            ParentId = e.ParentId,
            LanguageID = e.LanguageID,
            Domain = e.Domain,
            SiteId = e.SiteId,
            Creator = e.Creator,
            Created = e.Created
        };
    }
}
