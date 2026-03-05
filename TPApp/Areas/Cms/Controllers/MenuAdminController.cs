using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class MenuAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public MenuAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private string GetDomain() =>
            _configuration["AppSettings:MainDomain"]?.TrimEnd('/') ?? "";

        // ─────────────────────────────────────────
        // LIST
        // ─────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? keyword, int? statusId, int? parentId,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var siteId = GetSiteId();

            var query = _context.Menus.AsNoTracking()
                .Where(m => m.LanguageId == 1 && (m.SiteId == null || m.SiteId == siteId));

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(m => m.Title != null && m.Title.Contains(keyword));
            if (statusId.HasValue)
                query = query.Where(m => m.StatusId == statusId.Value);
            if (parentId.HasValue)
                query = query.Where(m => m.ParentId == parentId.Value);
            else
                query = query.Where(m => m.ParentId == null || m.ParentId == 0);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Sorting
            bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLower()) switch
            {
                "title" => asc ? query.OrderBy(m => m.Title) : query.OrderByDescending(m => m.Title),
                "sort" => asc ? query.OrderBy(m => m.Sort) : query.OrderByDescending(m => m.Sort),
                "status" => asc ? query.OrderBy(m => m.StatusId) : query.OrderByDescending(m => m.StatusId),
                "created" => asc ? query.OrderBy(m => m.Created) : query.OrderByDescending(m => m.Created),
                _ => query.OrderBy(m => m.Sort).ThenBy(m => m.Title)
            };

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MenuListItem
                {
                    MenuId = m.MenuId,
                    Title = m.Title ?? "",
                    ParentId = m.ParentId,
                    ParentTitle = m.ParentId.HasValue && m.ParentId.Value > 0
                        ? _context.Menus.Where(p => p.MenuId == m.ParentId.Value)
                            .Select(p => p.Title).FirstOrDefault()
                        : null,
                    Sort = m.Sort,
                    StatusId = m.StatusId,
                    StatusTitle = m.StatusId.HasValue
                        ? _context.Statuses.Where(s => s.StatusId == m.StatusId.Value)
                            .Select(s => s.Title).FirstOrDefault()
                        : null,
                    MenuPosition = m.MenuPosition,
                    QueryString = m.QueryString,
                    NavigateUrl = m.NavigateUrl,
                    Created = m.Created,
                    Creator = m.Creator,
                    Modified = m.Modified,
                    SiteId = m.SiteId
                })
                .ToListAsync();

            // ViewBag
            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.ParentId = parentId;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            ViewBag.Statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => new { s.StatusId, s.Title })
                .ToListAsync();

            // Parent menu tree for filter
            var filterMenus = await _context.Menus.AsNoTracking()
                .Where(m => m.LanguageId == 1 && (m.SiteId == null || m.SiteId == siteId))
                .OrderBy(m => m.Sort)
                .ToListAsync();
            ViewBag.ParentMenus = BuildMenuTree(filterMenus, null, 0);

            // Breadcrumb info for drill-down
            if (parentId.HasValue && parentId.Value > 0)
            {
                var browseParent = await _context.Menus.AsNoTracking()
                    .Where(m => m.MenuId == parentId.Value)
                    .Select(m => new { m.MenuId, m.Title, m.ParentId })
                    .FirstOrDefaultAsync();
                ViewBag.BrowseParentTitle = browseParent?.Title;
                ViewBag.BrowseParentParentId = browseParent?.ParentId;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        // ─────────────────────────────────────────
        // CREATE (GET)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new MenuFormVm
            {
                StatusId = 1,
                LanguageId = 1,
                Domain = GetDomain(),
                SiteId = GetSiteId(),
                Sort = 0
            };
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────
        // CREATE (POST)
        // ─────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = MapToEntity(vm);
            entity.Created = DateTime.Now;
            entity.Creator = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

            _context.Menus.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm menu thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────
        // EDIT (GET)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.Menus.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToVm(entity);
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────
        // EDIT (POST)
        // ─────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MenuFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = await _context.Menus.FindAsync(vm.MenuId);
            if (entity == null) return NotFound();

            entity.Title = vm.Title;
            entity.Description = vm.Description;
            entity.Sort = vm.Sort;
            entity.MenuPosition = vm.MenuPosition;
            entity.StatusId = (byte?)vm.StatusId;
            entity.ParentId = vm.ParentId;
            entity.QueryString = vm.QueryString;
            entity.NavigateUrl = vm.NavigateUrl;
            entity.ShowRight = (byte?)(vm.ShowRight ? 1 : 0);
            entity.TitlePage = vm.TitlePage;
            entity.ImageLogo = vm.ImageLogo;
            entity.bEffectiveDate = vm.bEffectiveDate;
            entity.eEffectiveDate = vm.eEffectiveDate;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật menu thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────
        // UPDATE SORT (drag & drop)
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateSort([FromBody] List<SortUpdateItem> items)
        {
            if (items == null || !items.Any())
                return Json(new { success = false, message = "Không có dữ liệu." });

            foreach (var item in items)
            {
                var entity = await _context.Menus.FindAsync(item.Id);
                if (entity != null)
                    entity.Sort = item.Sort;
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật thứ tự." });
        }

        // ─────────────────────────────────────────
        // DELETE (POST)
        // ─────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Menus.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy menu." });

            // Don't delete published menus
            if (entity.StatusId == 3)
                return Json(new { success = false, message = "Không thể xóa menu đang xuất bản." });

            // Check if menu has children
            var hasChildren = await _context.Menus.AnyAsync(m => m.ParentId == id);
            if (hasChildren)
                return Json(new { success = false, message = "Không thể xóa menu có menu con." });

            // Check if menu has content
            var hasContent = await _context.Contents.AnyAsync(c => c.MenuId == id);
            if (hasContent)
                return Json(new { success = false, message = "Không thể xóa menu đang có bài viết." });

            _context.Menus.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa menu thành công." });
        }

        // ─────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────
        private async Task LoadFormSelectListsAsync()
        {
            var siteId = GetSiteId();

            ViewBag.Statuses = new SelectList(
                await _context.Statuses.AsNoTracking().OrderBy(s => s.StatusId).ToListAsync(),
                "StatusId", "Title");

            var menus = await _context.Menus.AsNoTracking()
                .Where(m => m.LanguageId == 1 && (m.SiteId == null || m.SiteId == siteId))
                .OrderBy(m => m.Sort)
                .ToListAsync();

            var menuItems = BuildMenuTree(menus, null, 0);
            ViewBag.ParentMenus = new SelectList(menuItems, "Value", "Text");
        }

        private List<SelectListItem> BuildMenuTree(List<Menu> allMenus, int? parentId, int level)
        {
            var result = new List<SelectListItem>();
            var children = allMenus.Where(m =>
                parentId == null
                    ? (m.ParentId == null || m.ParentId == 0)
                    : m.ParentId == parentId
            ).ToList();
            foreach (var child in children)
            {
                var prefix = level > 0 ? new string(' ', (level - 1) * 4) + "|__" : "";
                result.Add(new SelectListItem
                {
                    Value = child.MenuId.ToString(),
                    Text = prefix + child.Title
                });
                result.AddRange(BuildMenuTree(allMenus, child.MenuId, level + 1));
            }
            return result;
        }

        private static Menu MapToEntity(MenuFormVm vm) => new()
        {
            Title = vm.Title,
            Description = vm.Description,
            Sort = vm.Sort,
            MenuPosition = vm.MenuPosition,
            StatusId = (byte?)vm.StatusId,
            ParentId = vm.ParentId,
            QueryString = vm.QueryString,
            NavigateUrl = vm.NavigateUrl,
            ShowRight = (byte?)(vm.ShowRight ? 1 : 0),
            TitlePage = vm.TitlePage,
            LanguageId = vm.LanguageId,
            ImageLogo = vm.ImageLogo,
            Domain = vm.Domain ?? "",
            SiteId = vm.SiteId,
            bEffectiveDate = vm.bEffectiveDate,
            eEffectiveDate = vm.eEffectiveDate
        };

        private static MenuFormVm MapToVm(Menu m) => new()
        {
            MenuId = m.MenuId,
            Title = m.Title,
            Description = m.Description,
            Sort = m.Sort,
            MenuPosition = m.MenuPosition,
            StatusId = m.StatusId ?? 1,
            ParentId = m.ParentId,
            QueryString = m.QueryString,
            NavigateUrl = m.NavigateUrl,
            ShowRight = m.ShowRight == 1,
            TitlePage = m.TitlePage,
            LanguageId = m.LanguageId,
            ImageLogo = m.ImageLogo,
            Domain = m.Domain,
            SiteId = m.SiteId,
            bEffectiveDate = m.bEffectiveDate,
            eEffectiveDate = m.eEffectiveDate,
            Created = m.Created,
            Creator = m.Creator
        };
    }

    // ─────────────────────────────────────────
    // DTOs
    // ─────────────────────────────────────────
    public class MenuListItem
    {
        public int MenuId { get; set; }
        public string Title { get; set; } = "";
        public int? ParentId { get; set; }
        public string? ParentTitle { get; set; }
        public int? Sort { get; set; }
        public byte? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public string? MenuPosition { get; set; }
        public string? QueryString { get; set; }
        public string? NavigateUrl { get; set; }
        public DateTime? Created { get; set; }
        public string? Creator { get; set; }
        public DateTime? Modified { get; set; }
        public int? SiteId { get; set; }
    }

    public class MenuFormVm
    {
        public int MenuId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tiêu đề không được để trống.")]
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }
        public int? Sort { get; set; }
        public string? MenuPosition { get; set; }
        public int StatusId { get; set; } = 1;
        public int? ParentId { get; set; }
        public string? QueryString { get; set; }
        public string? NavigateUrl { get; set; }
        public bool ShowRight { get; set; }
        public string? TitlePage { get; set; }
        public int LanguageId { get; set; } = 1;
        public string? ImageLogo { get; set; }
        public string? Domain { get; set; }
        public int? SiteId { get; set; }
        public DateTime? bEffectiveDate { get; set; }
        public DateTime? eEffectiveDate { get; set; }

        // Read-only display
        public DateTime? Created { get; set; }
        public string? Creator { get; set; }
    }
}
