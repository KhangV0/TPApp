using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Areas.Cms.Controllers
{
    // ── DTOs ──
    public class CategoryListItem
    {
        public int CatId { get; set; }
        public string? Title { get; set; }
        public string? ParentTitle { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public int? Sort { get; set; }
        public string? QueryString { get; set; }
        public string? LogoURL { get; set; }
        public bool? MainCate { get; set; }
        public string? Creator { get; set; }
        public DateTime? Created { get; set; }
    }

    public class SortUpdateItem
    {
        public int Id { get; set; }
        public int Sort { get; set; }
    }

    public class CategoryFormVm
    {
        public int CatId { get; set; }
        public string? Title { get; set; }
        public string? TitleEn { get; set; }
        public string? QueryString { get; set; }
        public int? ParentId { get; set; }
        public int? StatusId { get; set; }
        public int? Sort { get; set; }
        public string? Description { get; set; }
        public string? LogoURL { get; set; }
        public bool? MainCate { get; set; }
        public int LanguageId { get; set; } = 1;
        public string Domain { get; set; } = string.Empty;
        public int? SiteId { get; set; }
        public string? Creator { get; set; }
        public DateTime? Created { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize]
    public class CategoryAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public CategoryAdminController(AppDbContext context, IConfiguration configuration)
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
            string? keyword, int? statusId, int? parentId,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var siteId = GetSiteId();

            var query = _context.Categories.AsNoTracking()
                .Where(c => c.LanguageId == 1 && (c.SiteId == null || c.SiteId == siteId));

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.Title != null && c.Title.Contains(keyword));
            if (statusId.HasValue)
                query = query.Where(c => c.StatusId == statusId.Value);
            if (parentId.HasValue)
                query = query.Where(c => c.ParentId == parentId.Value);
            else
                query = query.Where(c => c.ParentId == null || c.ParentId == 0);

            // Sort
            query = sortBy?.ToLower() switch
            {
                "title" => sortDir == "desc" ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
                "sort" => sortDir == "desc" ? query.OrderByDescending(c => c.Sort) : query.OrderBy(c => c.Sort),
                "status" => sortDir == "desc" ? query.OrderByDescending(c => c.StatusId) : query.OrderBy(c => c.StatusId),
                "created" => sortDir == "desc" ? query.OrderByDescending(c => c.Created) : query.OrderBy(c => c.Created),
                _ => query.OrderBy(c => c.Sort).ThenByDescending(c => c.Created)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            // Load all categories for parent lookup
            var allCats = await _context.Categories.AsNoTracking()
                .Where(c => c.LanguageId == 1 && (c.SiteId == null || c.SiteId == siteId))
                .Select(c => new { c.CatId, c.Title })
                .ToListAsync();
            var catDict = allCats.ToDictionary(c => c.CatId, c => c.Title);

            // Statuses
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();

            var statusDict = statuses.ToDictionary(s => s.StatusId, s => s.Title);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryListItem
                {
                    CatId = c.CatId,
                    Title = c.Title,
                    StatusId = c.StatusId,
                    Sort = c.Sort,
                    QueryString = c.QueryString,
                    LogoURL = c.LogoURL,
                    MainCate = c.MainCate,
                    Creator = c.Creator,
                    Created = c.Created
                })
                .ToListAsync();

            // Map parent & status titles in memory
            foreach (var item in items)
            {
                if (item.StatusId.HasValue && statusDict.TryGetValue(item.StatusId.Value, out var sTitle))
                    item.StatusTitle = sTitle;
            }

            // Get parent IDs for current page items
            var itemIds = items.Select(i => i.CatId).ToList();
            var parentInfo = await _context.Categories.AsNoTracking()
                .Where(c => itemIds.Contains(c.CatId) && c.ParentId != null && c.ParentId != 0)
                .Select(c => new { c.CatId, c.ParentId })
                .ToListAsync();
            foreach (var p in parentInfo)
            {
                var item = items.FirstOrDefault(i => i.CatId == p.CatId);
                if (item != null && p.ParentId.HasValue && catDict.TryGetValue(p.ParentId.Value, out var pTitle))
                    item.ParentTitle = pTitle;
            }

            // Build parent menu tree for filter
            var allCatEntities = await _context.Categories.AsNoTracking()
                .Where(c => c.LanguageId == 1 && (c.SiteId == null || c.SiteId == siteId))
                .OrderBy(c => c.Sort)
                .ToListAsync();
            var parentMenuItems = BuildCategoryTree(allCatEntities, null, 0);

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.ParentId = parentId;
            ViewBag.Statuses = statuses;
            ViewBag.ParentCategories = parentMenuItems;

            // Breadcrumb info for drill-down
            if (parentId.HasValue && parentId.Value > 0)
            {
                var browseParent = await _context.Categories.AsNoTracking()
                    .Where(c => c.CatId == parentId.Value)
                    .Select(c => new { c.CatId, c.Title, c.ParentId })
                    .FirstOrDefaultAsync();
                ViewBag.BrowseParentTitle = browseParent?.Title;
                ViewBag.BrowseParentParentId = browseParent?.ParentId;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        // ── CREATE GET ──
        public async Task<IActionResult> Create()
        {
            var vm = new CategoryFormVm
            {
                LanguageId = 1,
                Domain = GetDomain(),
                SiteId = GetSiteId(),
                StatusId = 1,
                Sort = 0
            };
            await LoadFormSelectListsAsync(vm.SiteId ?? GetSiteId(), null);
            return View(vm);
        }

        // ── CREATE POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync(vm.SiteId ?? GetSiteId(), null);
                return View(vm);
            }

            var entity = MapToEntity(vm);
            entity.Created = DateTime.Now;
            entity.Creator = User.Identity?.Name;

            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ──
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToVm(entity);
            await LoadFormSelectListsAsync(vm.SiteId ?? GetSiteId(), id);
            return View(vm);
        }

        // ── EDIT POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync(vm.SiteId ?? GetSiteId(), vm.CatId);
                return View(vm);
            }

            var entity = await _context.Categories.FindAsync(vm.CatId);
            if (entity == null) return NotFound();

            entity.Title = vm.Title;
            entity.TitleEn = vm.TitleEn;
            entity.QueryString = vm.QueryString;
            entity.ParentId = vm.ParentId;
            entity.StatusId = vm.StatusId;
            entity.Sort = vm.Sort;
            entity.Description = vm.Description;
            entity.LogoURL = vm.LogoURL;
            entity.MainCate = vm.MainCate;
            entity.SiteId = vm.SiteId;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── UPDATE SORT (drag & drop) ──
        [HttpPost]
        public async Task<IActionResult> UpdateSort([FromBody] List<SortUpdateItem> items)
        {
            if (items == null || !items.Any())
                return Json(new { success = false, message = "Không có dữ liệu." });

            foreach (var item in items)
            {
                var entity = await _context.Categories.FindAsync(item.Id);
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
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy danh mục." });

            if (entity.StatusId == 3)
                return Json(new { success = false, message = "Không thể xóa danh mục đang xuất bản." });

            var hasChildren = await _context.Categories.AnyAsync(c => c.ParentId == id);
            if (hasChildren)
                return Json(new { success = false, message = "Không thể xóa danh mục có danh mục con." });

            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa danh mục #" + id });
        }

        // ── HELPERS ──
        private async Task LoadFormSelectListsAsync(int siteId, int? excludeId)
        {
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "Title");

            var cats = await _context.Categories.AsNoTracking()
                .Where(c => c.LanguageId == 1 && (c.SiteId == null || c.SiteId == siteId))
                .OrderBy(c => c.Sort)
                .ToListAsync();

            if (excludeId.HasValue)
                cats = cats.Where(c => c.CatId != excludeId.Value).ToList();

            var catItems = BuildCategoryTree(cats, null, 0);
            ViewBag.ParentCategories = new SelectList(catItems, "Value", "Text");
        }

        private List<SelectListItem> BuildCategoryTree(List<Category> allCats, int? parentId, int level)
        {
            var result = new List<SelectListItem>();
            var children = allCats.Where(c =>
                parentId == null
                    ? (c.ParentId == null || c.ParentId == 0)
                    : c.ParentId == parentId
            ).ToList();

            foreach (var child in children)
            {
                var prefix = level > 0 ? new string(' ', (level - 1) * 4) + "|__" : "";
                result.Add(new SelectListItem
                {
                    Value = child.CatId.ToString(),
                    Text = prefix + child.Title
                });
                result.AddRange(BuildCategoryTree(allCats, child.CatId, level + 1));
            }
            return result;
        }

        private Category MapToEntity(CategoryFormVm vm) => new()
        {
            Title = vm.Title,
            TitleEn = vm.TitleEn,
            QueryString = vm.QueryString,
            ParentId = vm.ParentId,
            StatusId = vm.StatusId,
            Sort = vm.Sort,
            Description = vm.Description,
            LogoURL = vm.LogoURL,
            MainCate = vm.MainCate,
            LanguageId = vm.LanguageId,
            Domain = vm.Domain,
            SiteId = vm.SiteId
        };

        private CategoryFormVm MapToVm(Category e) => new()
        {
            CatId = e.CatId,
            Title = e.Title,
            TitleEn = e.TitleEn,
            QueryString = e.QueryString,
            ParentId = e.ParentId,
            StatusId = e.StatusId,
            Sort = e.Sort,
            Description = e.Description,
            LogoURL = e.LogoURL,
            MainCate = e.MainCate,
            LanguageId = e.LanguageId,
            Domain = e.Domain,
            SiteId = e.SiteId,
            Creator = e.Creator,
            Created = e.Created
        };
    }
}
