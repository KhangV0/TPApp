using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Text;
using TPApp.Areas.Cms.Controllers;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class QuanLySanPhamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ICntbMasterService _masterService;

        public QuanLySanPhamController(AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ICntbMasterService masterService)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _masterService = masterService;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private async Task LoadFormSelectListsAsync()
        {
            ViewBag.XuatXuList     = ToSelectList(await _masterService.GetXuatXuAsync(),    "--- Chọn xuất xứ ---");
            ViewBag.MucDoList      = ToSelectList(await _masterService.GetMucDoAsync(),     "--- Chọn mức độ ---");
            ViewBag.LinhVucList    = ToSelectList(await _masterService.GetLinhVucAsync(),   "--- Chọn lĩnh vực ---");
            ViewBag.NhaCungUngList = ToSelectList(await _masterService.GetNhaCungUngAsync(), "--- Chọn nhà cung ứng ---");
        }

        private static List<SelectListItem> ToSelectList(
            List<TPApp.Areas.Cms.Models.LookupDto> items, string defaultText)
        {
            var list = new List<SelectListItem> { new SelectListItem(defaultText, "") };
            list.AddRange(items.Select(x => new SelectListItem(x.Title, x.Id.ToString())));
            return list;
        }

        private static string ToSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant()
                         .Replace('đ', 'd').Replace('Đ', 'd');
            slug = Regex.Replace(slug, @"[^a-z0-9]+", "-").Trim('-');
            return Regex.Replace(slug, @"-{2,}", "-");
        }

        // ─────────────────────────────────────────────
        // INDEX (danh sách sản phẩm của user)
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int? statusId, int? productType,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 15)
        {
            var creator = User.Identity?.Name ?? "";
            var query = _context.SanPhamCNTBs.Where(p => p.Creator == creator).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name != null && p.Name.Contains(keyword));
            if (statusId.HasValue)
                query = query.Where(p => p.StatusId == statusId);
            if (productType.HasValue)
                query = query.Where(p => p.ProductType == productType.Value);

            bool asc = sortDir?.ToLower() != "desc";
            query = sortBy?.ToLower() switch
            {
                "name"    => asc ? query.OrderBy(p => p.Name)     : query.OrderByDescending(p => p.Name),
                "created" => asc ? query.OrderBy(p => p.Created)  : query.OrderByDescending(p => p.Created),
                "status"  => asc ? query.OrderBy(p => p.StatusId) : query.OrderByDescending(p => p.StatusId),
                _         => query.OrderByDescending(p => p.Created)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new SanPhamCNTBListItem
                {
                    ID          = p.ID,
                    Code        = p.Code ?? "",
                    Name        = p.Name ?? "",
                    StatusId    = p.StatusId,
                    Creator     = p.Creator,
                    Created     = p.Created,
                    LastUpdate  = p.Modified,
                    Viewed      = p.Viewed ?? 0,
                    ProductType = p.ProductType,
                    SiteId      = p.SiteId,
                    PublicUrl   = p.URL,
                })
                .ToListAsync();

            var statusIds = items.Where(i => i.StatusId.HasValue).Select(i => i.StatusId!.Value).Distinct().ToList();
            var statusMap = await _context.Statuses
                .Where(s => statusIds.Contains(s.StatusId))
                .ToDictionaryAsync(s => s.StatusId, s => s.Title);
            foreach (var item in items)
                if (item.StatusId.HasValue && statusMap.TryGetValue(item.StatusId.Value, out var st))
                    item.StatusTitle = st;

            ViewBag.Keyword     = keyword;
            ViewBag.StatusId    = statusId;
            ViewBag.ProductType = productType;
            ViewBag.SortBy      = sortBy;
            ViewBag.SortDir     = sortDir;
            ViewBag.Page        = page;
            ViewBag.PageSize    = pageSize;
            ViewBag.TotalCount  = totalCount;
            ViewBag.TotalPages  = totalPages;

            return View(items);
        }

        // ─────────────────────────────────────────────
        // CREATE — Công nghệ (GET)
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> TaoMoiCongNghe()
        {
            var maxId = await _context.SanPhamCNTBs.Where(x => x.ProductType == 1).MaxAsync(x => (int?)x.ID) ?? 0;
            var vm = new SanPhamCNTBFormVm { ProductType = 1, Code = $"CN-{(maxId + 1):D5}", StatusId = 1, SiteId = GetSiteId() };
            ViewData["Title"] = "Đăng ký Công nghệ mới";
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────────
        // CREATE — Thiết bị (GET)
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> TaoMoiThietBi()
        {
            var maxId = await _context.SanPhamCNTBs.Where(x => x.ProductType == 2).MaxAsync(x => (int?)x.ID) ?? 0;
            var vm = new SanPhamCNTBFormVm { ProductType = 2, Code = $"TB-{(maxId + 1):D5}", StatusId = 1, SiteId = GetSiteId() };
            ViewData["Title"] = "Đăng ký Thiết bị mới";
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────────
        // CREATE — Sở hữu trí tuệ (GET)
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> TaoMoiSoHuuTriTue()
        {
            var maxId = await _context.SanPhamCNTBs.Where(x => x.ProductType == 3).MaxAsync(x => (int?)x.ID) ?? 0;
            var vm = new SanPhamCNTBFormVm { ProductType = 3, Code = $"TT-{(maxId + 1):D5}", StatusId = 1, SiteId = GetSiteId() };
            ViewData["Title"] = "Đăng ký Sở hữu trí tuệ mới";
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────────
        // CREATE — POST (chung cho cả 3 loại)
        // ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(ValueLengthLimit = 52_428_800, MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> TaoMoi(SanPhamCNTBFormVm model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(GetCreateViewName(model.ProductType), model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = SanPhamCNTBController.BuildEntity(model);
                entity.Created     = DateTime.Now;
                entity.SiteId      = GetSiteId();
                entity.StatusId    = 1; // Chờ duyệt
                entity.Creator     = User.Identity?.Name;
                entity.LanguageId  = 1;

                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    var prefix = model.ProductType switch { 2 => "TB", 3 => "TT", _ => "CN" };
                    var maxId  = await _context.SanPhamCNTBs
                        .Where(x => x.ProductType == model.ProductType)
                        .MaxAsync(x => (int?)x.ID) ?? 0;
                    entity.Code = $"{prefix}-{(maxId + 1):D5}";
                }
                entity.QueryString = ToSlug(entity.Name ?? "");

                _context.SanPhamCNTBs.Add(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Đã đăng ký '{entity.Name}' thành công! Hồ sơ đang chờ xét duyệt.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                await LoadFormSelectListsAsync();
                return View(GetCreateViewName(model.ProductType), model);
            }
        }

        // ─────────────────────────────────────────────
        // EDIT (GET)
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ChinhSua(int id)
        {
            var entity = await _context.SanPhamCNTBs.FindAsync(id);
            if (entity == null) return NotFound();
            if (entity.Creator != User.Identity?.Name) return Forbid();

            var vm = SanPhamCNTBController.BuildFormVm(entity);
            ViewData["Title"] = $"Chỉnh sửa: {entity.Name}";
            await LoadFormSelectListsAsync();
            return View(GetEditViewName(entity.ProductType), vm);
        }

        // ─────────────────────────────────────────────
        // EDIT (POST)
        // ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(ValueLengthLimit = 52_428_800, MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> ChinhSua(SanPhamCNTBFormVm model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(GetEditViewName(model.ProductType), model);
            }

            var entity = await _context.SanPhamCNTBs.FindAsync(model.ID);
            if (entity == null) return NotFound();
            if (entity.Creator != User.Identity?.Name) return Forbid();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldStatusId = entity.StatusId;
                SanPhamCNTBController.ApplyEdit(entity, model);
                entity.Modified    = DateTime.Now;
                entity.Modifier    = User.Identity?.Name;
                // Nếu đang ở trạng thái Đã duyệt (3) → chuyển về Đang duyệt (2)
                // Ngược lại giữ nguyên trạng thái hiện tại
                entity.StatusId    = oldStatusId == 3 ? 2 : oldStatusId;
                entity.QueryString = ToSlug(entity.Name ?? "");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Cập nhật thành công! Hồ sơ đang chờ xét duyệt lại.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                await LoadFormSelectListsAsync();
                return View(GetEditViewName(model.ProductType), model);
            }
        }

        // ─────────────────────────────────────────────
        // DELETE (AJAX POST)
        // ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var entity = await _context.SanPhamCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            if (entity.Creator != User.Identity?.Name)
                return Json(new { success = false, message = "Bạn không có quyền xóa sản phẩm này." });
            if (entity.StatusId == 3)
                return Json(new { success = false, message = "Không thể xóa sản phẩm đã được duyệt." });

            try
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SanPhamCNTBCategory WHERE SanPhamCNTBId = {0}", id);
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SanPhamCNTBMedia WHERE SanPhamCNTBId = {0}", id);
                _context.SanPhamCNTBs.Remove(entity);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa sản phẩm." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ─── Helpers ───
        private string GetCreateViewName(int pt) => pt switch { 2 => "TaoMoiThietBi",   3 => "TaoMoiSoHuuTriTue",   _ => "TaoMoiCongNghe"   };
        private string GetEditViewName(int pt)   => pt switch { 2 => "TaoMoiThietBi",   3 => "TaoMoiSoHuuTriTue",   _ => "TaoMoiCongNghe"   };
    }
}
