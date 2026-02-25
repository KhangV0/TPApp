using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Areas.Cms.Models;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;

namespace TPApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class PostsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PostsController(AppDbContext context, IConfiguration configuration)
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
            string? keyword, int? menuId, int? statusId, int? typeId,
            bool? isHot, bool? isNew,
            DateTime? dateFrom, DateTime? dateTo,
            int? siteId,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 20)
        {
            ViewData["Title"] = "Quản lý Tin bài";

            if (!siteId.HasValue)
                siteId = GetSiteId();

            var query = _context.Contents.AsNoTracking().AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => (c.Title != null && c.Title.Contains(keyword))
                    || (c.Keyword != null && c.Keyword.Contains(keyword)));
            if (menuId.HasValue)
                query = query.Where(c => c.MenuId == menuId.Value);
            if (statusId.HasValue)
                query = query.Where(c => c.StatusId == statusId.Value);
            if (typeId.HasValue)
                query = query.Where(c => c.TypeId == typeId.Value);
            if (isHot == true)
                query = query.Where(c => c.IsHot == true);
            if (isNew == true)
                query = query.Where(c => c.IsNew == true);
            if (dateFrom.HasValue)
                query = query.Where(c => c.PublishedDate >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(c => c.PublishedDate <= dateTo.Value.AddDays(1));
            if (siteId.HasValue)
                query = query.Where(c => c.SiteId == siteId.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Sorting
            bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLower()) switch
            {
                "title" => asc ? query.OrderBy(c => c.Title) : query.OrderByDescending(c => c.Title),
                "menu" => asc ? query.OrderBy(c => c.MenuId) : query.OrderByDescending(c => c.MenuId),
                "status" => asc ? query.OrderBy(c => c.StatusId) : query.OrderByDescending(c => c.StatusId),
                "published" => asc ? query.OrderBy(c => c.PublishedDate) : query.OrderByDescending(c => c.PublishedDate),
                "viewed" => asc ? query.OrderBy(c => c.Viewed) : query.OrderByDescending(c => c.Viewed),
                "created" => asc ? query.OrderBy(c => c.Created) : query.OrderByDescending(c => c.Created),
                _ => query.OrderByDescending(c => c.PublishedDate ?? c.Created)
            };

            // Projection
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContentListItem
                {
                    Id = c.Id,
                    Title = c.Title ?? "",
                    MenuId = c.MenuId,
                    MenuTitle = c.MenuId.HasValue
                        ? _context.Menus.Where(m => m.MenuId == c.MenuId.Value)
                            .Select(m => m.Title).FirstOrDefault()
                        : null,
                    StatusId = c.StatusId,
                    StatusTitle = c.StatusId.HasValue
                        ? _context.Statuses.Where(s => s.StatusId == c.StatusId.Value)
                            .Select(s => s.Title).FirstOrDefault()
                        : null,
                    PublishedDate = c.PublishedDate,
                    Created = c.Created,
                    Modified = c.Modified,
                    Viewed = c.Viewed ?? 0,
                    IsHot = c.IsHot ?? false,
                    IsNew = c.IsNew ?? false,
                    Author = c.Author,
                    Image = c.Image,
                    Creator = c.Creator,
                    SiteId = c.SiteId
                })
                .ToListAsync();

            // ViewBag for filters
            ViewBag.Keyword = keyword;
            ViewBag.MenuId = menuId;
            ViewBag.StatusId = statusId;
            ViewBag.TypeId = typeId;
            ViewBag.IsHot = isHot;
            ViewBag.IsNew = isNew;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
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
            var filterMenus = await _context.Menus.AsNoTracking()
                .Where(m => m.LanguageId == 1 && (m.SiteId == null || m.SiteId == siteId))
                .OrderBy(m => m.Sort)
                .ToListAsync();
            ViewBag.Menus = BuildMenuTree(filterMenus, null, 0);
            ViewBag.SiteId = siteId;
            ViewBag.CurrentSiteId = GetSiteId();
            ViewBag.Sites = await _context.RootSites.AsNoTracking()
                .OrderBy(s => s.SiteId)
                .ToListAsync();

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
            var vm = new ContentFormVm
            {
                StatusId = 1,
                LanguageId = 1,
                Domain = GetDomain(),
                SiteId = GetSiteId(),
                PublishedDate = DateTime.Now
            };
            ViewData["Title"] = "Thêm tin bài";
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────
        // CREATE (POST)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContentFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                ViewData["Title"] = "Thêm tin bài";
                return View(vm);
            }

            // Auto-generate slug if empty
            if (string.IsNullOrWhiteSpace(vm.QueryString))
                vm.QueryString = SlugHelper.Slugify(vm.Title);

            var entity = MapToEntity(vm);
            entity.Created = DateTime.Now;
            entity.Creator = User.Identity?.Name;

            _context.Contents.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã thêm tin bài thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────
        // EDIT (GET)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var entity = await _context.Contents.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToFormVm(entity);
            ViewData["Title"] = $"Sửa: {entity.Title}";
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────
        // EDIT (POST)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContentFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                ViewData["Title"] = $"Sửa: {vm.Title}";
                return View(vm);
            }

            var entity = await _context.Contents.FindAsync(vm.Id);
            if (entity == null) return NotFound();

            // Auto-generate slug if empty
            if (string.IsNullOrWhiteSpace(vm.QueryString))
                vm.QueryString = SlugHelper.Slugify(vm.Title);

            UpdateEntity(entity, vm);
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật tin bài thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────
        // DETAIL
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Detail(long id)
        {
            var entity = await _context.Contents.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return NotFound();

            var vm = MapToFormVm(entity);

            // Menu title
            if (entity.MenuId.HasValue)
            {
                ViewBag.MenuTitle = await _context.Menus
                    .Where(m => m.MenuId == entity.MenuId.Value)
                    .Select(m => m.Title)
                    .FirstOrDefaultAsync() ?? "(không xác định)";
            }

            // Status title
            if (entity.StatusId.HasValue)
            {
                ViewBag.StatusTitle = await _context.Statuses
                    .Where(s => s.StatusId == entity.StatusId.Value)
                    .Select(s => s.Title)
                    .FirstOrDefaultAsync() ?? "(không xác định)";
            }

            ViewData["Title"] = $"Chi tiết: {entity.Title}";
            return View(vm);
        }

        // ─────────────────────────────────────────
        // DELETE (POST AJAX)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.Contents.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy bài viết." });

            _context.Contents.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã xóa bài viết #{id}." });
        }

        // ─────────────────────────────────────────
        // QUICK CONFIG (GET – load partial)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> QuickConfig(long id)
        {
            var entity = await _context.Contents.FindAsync(id);
            if (entity == null) return NotFound();

            ViewBag.Statuses = new SelectList(
                await _context.Statuses.AsNoTracking()
                    .OrderBy(s => s.StatusId)
                    .ToListAsync(),
                "StatusId", "Title", entity.StatusId);

            return PartialView("_QuickConfigPartial", entity);
        }

        // ─────────────────────────────────────────
        // QUICK CONFIG (POST – save)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickConfig(long id, int? statusId, bool isHot, bool isNew)
        {
            var entity = await _context.Contents.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy." });

            entity.StatusId = statusId;
            entity.IsHot = isHot;
            entity.IsNew = isNew;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã cập nhật cấu hình." });
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

            // Build flat menu list with indented titles for parent > child display
            // Filter by SiteId to only show menus belonging to current site
            var menus = await _context.Menus.AsNoTracking()
                .Where(m => m.LanguageId == 1 && (m.SiteId == null || m.SiteId == siteId))
                .OrderBy(m => m.Sort)
                .ToListAsync();

            var menuItems = BuildMenuTree(menus, null, 0);
            ViewBag.Menus = new SelectList(menuItems, "Value", "Text");
        }

        private List<SelectListItem> BuildMenuTree(List<Menu> allMenus, int? parentId, int level)
        {
            var result = new List<SelectListItem>();
            // Root menus may have ParentId = 0 or ParentId = null
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

        private static Content MapToEntity(ContentFormVm vm)
        {
            return new Content
            {
                Title = vm.Title,
                QueryString = vm.QueryString,
                Description = vm.Description,
                Contents = vm.Contents,
                Author = vm.Author,
                StatusId = vm.StatusId,
                MenuId = vm.MenuId,
                IsHot = vm.IsHot,
                IsNew = vm.IsNew,
                Image = vm.Image,
                ImageBig = vm.ImageBig,
                PublishedDate = vm.PublishedDate,
                bEffectiveDate = vm.bEffectiveDate,
                eEffectiveDate = vm.eEffectiveDate,
                Subject = vm.Subject,
                Keyword = vm.Keyword,
                TypeId = vm.TypeId,
                URL = vm.URL,
                IsYoutube = vm.IsYoutube,
                LinkFile = vm.LinkFile,
                LinkInvite = vm.LinkInvite,
                LanguageId = vm.LanguageId,
                Domain = vm.Domain,
                SiteId = vm.SiteId,
                ParentId = vm.ParentId,
                ChuongTrinh = vm.ChuongTrinh,
                PhieuDangKy = vm.PhieuDangKy,
                STINFO = vm.STINFO,
                HinhSTINFO = vm.HinhSTINFO
            };
        }

        private static void UpdateEntity(Content entity, ContentFormVm vm)
        {
            entity.Title = vm.Title;
            entity.QueryString = vm.QueryString;
            entity.Description = vm.Description;
            entity.Contents = vm.Contents;
            entity.Author = vm.Author;
            entity.StatusId = vm.StatusId;
            entity.MenuId = vm.MenuId;
            entity.IsHot = vm.IsHot;
            entity.IsNew = vm.IsNew;
            entity.Image = vm.Image;
            entity.ImageBig = vm.ImageBig;
            entity.PublishedDate = vm.PublishedDate;
            entity.bEffectiveDate = vm.bEffectiveDate;
            entity.eEffectiveDate = vm.eEffectiveDate;
            entity.Subject = vm.Subject;
            entity.Keyword = vm.Keyword;
            entity.TypeId = vm.TypeId;
            entity.URL = vm.URL;
            entity.IsYoutube = vm.IsYoutube;
            entity.LinkFile = vm.LinkFile;
            entity.LinkInvite = vm.LinkInvite;
            entity.LanguageId = vm.LanguageId;
            entity.Domain = vm.Domain;
            entity.SiteId = vm.SiteId;
            entity.ParentId = vm.ParentId;
            entity.ChuongTrinh = vm.ChuongTrinh;
            entity.PhieuDangKy = vm.PhieuDangKy;
            entity.STINFO = vm.STINFO;
            entity.HinhSTINFO = vm.HinhSTINFO;
        }

        private static ContentFormVm MapToFormVm(Content entity)
        {
            return new ContentFormVm
            {
                Id = entity.Id,
                Title = entity.Title ?? "",
                QueryString = entity.QueryString,
                Description = entity.Description,
                Contents = entity.Contents,
                Author = entity.Author,
                StatusId = entity.StatusId,
                MenuId = entity.MenuId,
                IsHot = entity.IsHot ?? false,
                IsNew = entity.IsNew ?? false,
                Image = entity.Image,
                ImageBig = entity.ImageBig,
                PublishedDate = entity.PublishedDate,
                bEffectiveDate = entity.bEffectiveDate,
                eEffectiveDate = entity.eEffectiveDate,
                Subject = entity.Subject,
                Keyword = entity.Keyword,
                TypeId = entity.TypeId,
                URL = entity.URL,
                IsYoutube = entity.IsYoutube ?? false,
                LinkFile = entity.LinkFile,
                LinkInvite = entity.LinkInvite,
                LanguageId = entity.LanguageId,
                Domain = entity.Domain,
                SiteId = entity.SiteId,
                ParentId = entity.ParentId,
                ChuongTrinh = entity.ChuongTrinh,
                PhieuDangKy = entity.PhieuDangKy,
                STINFO = entity.STINFO,
                HinhSTINFO = entity.HinhSTINFO
            };
        }
    }
}
