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
            string? keyword, int? statusId, int? subject, int? siteId,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var currentSiteId = GetSiteId();

            // Default to current site; only SiteId=1 can switch
            if (!siteId.HasValue || currentSiteId != 1)
                siteId = currentSiteId;

            var query = _context.ImageAdvers.AsNoTracking()
                .Where(a => a.LanguageID == 1 && a.SiteId == siteId);

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

            // Site filter
            ViewBag.Sites = new SelectList(
                await _context.RootSites.AsNoTracking().ToListAsync(),
                "SiteId", "SiteName", siteId);
            ViewBag.CurrentSiteId = currentSiteId;
            ViewBag.SiteId = siteId;

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
            // Luôn lấy Domain và SiteId từ appsettings.json
            vm.Domain = GetDomain();
            vm.SiteId = GetSiteId();
            if (vm.LanguageID == 0)
                vm.LanguageID = 1;

            // Clear ModelState cho các field tự động
            ModelState.Remove("Domain");
            ModelState.Remove("SiteId");
            ModelState.Remove("LanguageID");

            if (string.IsNullOrWhiteSpace(vm.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại thông tin: " +
                    string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = MapToEntity(vm);
            entity.Created = DateTime.Now;
            entity.Creator = User.Identity?.Name;

            try
            {
                _context.ImageAdvers.Add(entity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã tạo quảng cáo thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lưu không thành công: " + ex.Message;
                await LoadFormSelectListsAsync();
                return View(vm);
            }
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
            // Domain và SiteId không được chỉnh sửa trong Edit — bỏ qua validation
            ModelState.Remove("Domain");
            ModelState.Remove("SiteId");

            if (string.IsNullOrWhiteSpace(vm.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại thông tin: " +
                    string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
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
            // Domain và SiteId giữ nguyên giá trị trong DB, không cập nhật từ form
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật quảng cáo thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lưu không thành công: " + ex.Message;
                await LoadFormSelectListsAsync();
                return View(vm);
            }
        }

        // ── UPDATE SORT (drag & drop) ──
        [HttpPost]
        public async Task<IActionResult> UpdateSort([FromBody] List<SortUpdateItem> items)
        {
            if (items == null || !items.Any())
                return Json(new { success = false, message = "Không có dữ liệu." });

            foreach (var item in items)
            {
                var entity = await _context.ImageAdvers.FindAsync(item.Id);
                if (entity != null)
                    entity.Sort = item.Sort;
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật thứ tự." });
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
