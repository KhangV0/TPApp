using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Areas.Cms.Models;
using TPApp.Controllers;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;
using TPApp.Services;

namespace TPApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SanPhamCNTBController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICntbMasterService _masterService;
        private readonly IConfiguration _configuration;
        private readonly IExcelExportService _excelExport;

        public SanPhamCNTBController(AppDbContext context, ICntbMasterService masterService, IConfiguration configuration, IExcelExportService excelExport)
        {
            _context = context;
            _masterService = masterService;
            _configuration = configuration;
            _excelExport = excelExport;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        // ─── Constants ───
        private const int TypeCongNghe = 1;
        private const int TypeThietBi = 2;
        private const int TypeSanPhamTriTue = 3;
        private const int LogFunctionId = 2; // SanPhamCNTB

        private async Task WriteLog(int eventId, string content)
        {
            _context.Logs.Add(new Entities.Log
            {
                FunctionID = LogFunctionId,
                ActTime = DateTime.Now,
                EventID = eventId,
                Content = content,
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserName = User.Identity?.Name,
                Domain = HttpContext.Request.Host.Value,
                LanguageId = 1,
                ParentId = 0,
                SiteId = GetSiteId()
            });
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // LIST: Công nghệ
        // ─────────────────────────────────────────
        [HttpGet]
        public Task<IActionResult> CongNghe(string? keyword, int? statusId, int? ncuId, int? xuatXuId, int? siteId,
            string? creator, string? linhVuc, int? trlLevel,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 15)
            => ListByType(TypeCongNghe, "CongNghe", "Quản lý Công nghệ",
                keyword, statusId, ncuId, xuatXuId, siteId, creator, linhVuc, trlLevel, null, createdFrom, createdTo, sortBy, sortDir, page, pageSize);

        // ─────────────────────────────────────────
        // LIST: Thiết bị
        // ─────────────────────────────────────────
        [HttpGet]
        public Task<IActionResult> ThietBi(string? keyword, int? statusId, int? ncuId, int? xuatXuId, int? siteId,
            string? creator, string? linhVuc,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 15)
            => ListByType(TypeThietBi, "ThietBi", "Quản lý Thiết bị",
                keyword, statusId, ncuId, xuatXuId, siteId, creator, linhVuc, null, null, createdFrom, createdTo, sortBy, sortDir, page, pageSize);

        // ─────────────────────────────────────────
        // LIST: Sản phẩm trí tuệ
        // ─────────────────────────────────────────
        [HttpGet]
        public Task<IActionResult> SanPhamTriTue(string? keyword, int? statusId, int? ncuId, int? xuatXuId, int? siteId,
            string? creator, string? linhVuc, string? categoryId,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 15)
            => ListByType(TypeSanPhamTriTue, "SanPhamTriTue", "Quản lý Sản phẩm trí tuệ",
                keyword, statusId, ncuId, xuatXuId, siteId, creator, linhVuc, null, categoryId, createdFrom, createdTo, sortBy, sortDir, page, pageSize);

        // ─────────────────────────────────────────
        // EXPORT EXCEL: Công nghệ
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            string? keyword, int? statusId, int? ncuId, int? xuatXuId, int? siteId,
            string? creator, string? linhVuc, int? trlLevel, string? categoryId,
            DateTime? createdFrom, DateTime? createdTo,
            int productType = TypeCongNghe)
        {
            if (!siteId.HasValue)
                siteId = GetSiteId();

            var query = _context.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.ProductType == productType);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => (p.Name != null && p.Name.Contains(keyword))
                    || (p.Code != null && p.Code.Contains(keyword)));
            if (statusId.HasValue)
                query = query.Where(p => p.StatusId == statusId.Value);
            if (ncuId.HasValue)
                query = query.Where(p => p.NCUId == ncuId.Value);
            if (xuatXuId.HasValue)
                query = query.Where(p => p.XuatXuId == xuatXuId.Value);
            if (!string.IsNullOrWhiteSpace(creator))
                query = query.Where(p => p.Creator != null && p.Creator.Contains(creator));
            if (!string.IsNullOrWhiteSpace(linhVuc))
                query = query.Where(p => p.CategoryId != null && p.CategoryId.Contains(linhVuc));
            if (trlLevel.HasValue)
                query = query.Where(p => p.TRLLevel == trlLevel.Value);
            if (!string.IsNullOrWhiteSpace(categoryId))
                query = query.Where(p => p.CategoryId != null && p.CategoryId.Contains(categoryId));
            if (createdFrom.HasValue)
                query = query.Where(p => p.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(p => p.Created <= createdTo.Value.AddDays(1));
            if (siteId.HasValue)
                query = query.Where(p => p.SiteId == siteId.Value);

            query = query.OrderByDescending(p => p.bEffectiveDate ?? p.Created);

            var items = await query
                .Select(p => new SanPhamCNTBListItem
                {
                    ID = p.ID,
                    Code = p.Code ?? "",
                    Name = p.Name ?? "",
                    StatusId = p.StatusId,
                    StatusTitle = _context.Statuses
                        .Where(s => s.StatusId == p.StatusId)
                        .Select(s => s.Title)
                        .FirstOrDefault(),
                    Creator = p.Creator,
                    Created = p.bEffectiveDate,
                    LastUpdate = p.Modified ?? p.Created,
                    Viewed = p.Viewed ?? 0,
                    ProductType = p.ProductType,
                    SoBang = p.SoBang,
                    NhaCungUngName = p.NCUId.HasValue
                        ? _context.NhaCungUngs
                            .Where(n => n.CungUngId == p.NCUId.Value)
                            .Select(n => n.FullName)
                            .FirstOrDefault()
                        : null,
                    SiteId = p.SiteId
                })
                .ToListAsync();

            // Build public URL for each item
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            foreach (var item in items)
            {
                var slug = ProductController.MakeURLFriendly(item.Name);
                item.PublicUrl = $"{baseUrl}/2-cong-nghe-thiet-bi/{item.ProductType}/{slug}-{item.ID}.html";
            }

            var typeName = productType switch
            {
                TypeCongNghe => "CongNghe",
                TypeThietBi => "ThietBi",
                TypeSanPhamTriTue => "SanPhamTriTue",
                _ => "Export"
            };

            return _excelExport.Export(items, $"{typeName}_{DateTime.Now:yyyyMMdd}");
        }

        // ─────────────────────────────────────────
        // Shared list logic (projection, no Include)
        // ─────────────────────────────────────────
        private async Task<IActionResult> ListByType(int productType, string viewName, string title,
            string? keyword, int? statusId, int? ncuId, int? xuatXuId, int? siteId,
            string? creator, string? linhVuc, int? trlLevel, string? categoryId,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page, int pageSize)
        {
            ViewData["Title"] = title;
            ViewData["ProductType"] = productType;

            // Default to current site if no filter specified
            if (!siteId.HasValue)
                siteId = GetSiteId();

            var query = _context.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.ProductType == productType);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => (p.Name != null && p.Name.Contains(keyword))
                    || (p.Code != null && p.Code.Contains(keyword)));
            if (statusId.HasValue)
                query = query.Where(p => p.StatusId == statusId.Value);
            if (ncuId.HasValue)
                query = query.Where(p => p.NCUId == ncuId.Value);
            if (xuatXuId.HasValue)
                query = query.Where(p => p.XuatXuId == xuatXuId.Value);
            if (!string.IsNullOrWhiteSpace(creator))
                query = query.Where(p => p.Creator != null && p.Creator.Contains(creator));
            if (!string.IsNullOrWhiteSpace(linhVuc))
                query = query.Where(p => p.CategoryId != null && p.CategoryId.Contains(linhVuc));
            if (trlLevel.HasValue)
                query = query.Where(p => p.TRLLevel == trlLevel.Value);
            if (!string.IsNullOrWhiteSpace(categoryId))
                query = query.Where(p => p.CategoryId != null && p.CategoryId.Contains(categoryId));
            if (createdFrom.HasValue)
                query = query.Where(p => p.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(p => p.Created <= createdTo.Value.AddDays(1));
            if (siteId.HasValue)
                query = query.Where(p => p.SiteId == siteId.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Dynamic sorting
            bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLower()) switch
            {
                "name" => asc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "code" => asc ? query.OrderBy(p => p.Code) : query.OrderByDescending(p => p.Code),
                "status" => asc ? query.OrderBy(p => p.StatusId) : query.OrderByDescending(p => p.StatusId),
                "creator" => asc ? query.OrderBy(p => p.Creator) : query.OrderByDescending(p => p.Creator),
                "lastupdate" => asc ? query.OrderBy(p => p.Modified ?? p.Created) : query.OrderByDescending(p => p.Modified ?? p.Created),
                "viewed" => asc ? query.OrderBy(p => p.Viewed) : query.OrderByDescending(p => p.Viewed),
                _ => query.OrderByDescending(p => p.bEffectiveDate ?? p.Created)
            };

            // Projection — no Include, load only IsMain image URL
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new SanPhamCNTBListItem
                {
                    ID = p.ID,
                    Code = p.Code ?? "",
                    Name = p.Name ?? "",
                    StatusId = p.StatusId,
                    StatusTitle = _context.Statuses
                        .Where(s => s.StatusId == p.StatusId)
                        .Select(s => s.Title)
                        .FirstOrDefault(),
                    Creator = p.Creator,
                    Created = p.bEffectiveDate,
                    LastUpdate = p.Modified ?? p.Created,
                    Viewed = p.Viewed ?? 0,
                    ProductType = p.ProductType,
                    SoBang = p.SoBang,
                    NhaCungUngName = p.NCUId.HasValue
                        ? _context.NhaCungUngs
                            .Where(n => n.CungUngId == p.NCUId.Value)
                            .Select(n => n.FullName)
                            .FirstOrDefault()
                        : null,
                    ImageUrl = p.QuyTrinhHinhAnh,
                    SiteId = p.SiteId
                })
                .ToListAsync();

            // Preserve search params
            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.NcuId = ncuId;
            ViewBag.XuatXuId = xuatXuId;
            ViewBag.Creator = creator;
            ViewBag.LinhVuc = linhVuc;
            ViewBag.TrlLevel = trlLevel;
            ViewBag.CategoryId = categoryId;
            ViewBag.CreatedFrom = createdFrom;
            ViewBag.CreatedTo = createdTo;
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

            // Dropdown lists for filters
            ViewBag.NhaCungUngs = await _context.NhaCungUngs.AsNoTracking()
                .OrderBy(n => n.FullName)
                .Select(n => new { n.CungUngId, n.FullName })
                .ToListAsync();
            ViewBag.XuatXus = await _masterService.GetXuatXuAsync();
            ViewBag.LinhVucs = await _masterService.GetLinhVucAsync();
            ViewBag.MucDos = await _masterService.GetMucDoAsync();
            ViewBag.SiteId = siteId;
            ViewBag.CurrentSiteId = GetSiteId();
            ViewBag.Sites = await _context.RootSites.AsNoTracking()
                .OrderBy(s => s.SiteId)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(viewName, items);
        }

        // ─────────────────────────────────────────
        // DETAIL
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var p = await _context.SanPhamCNTBs.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id);
            if (p == null) return NotFound();

            ViewData["Title"] = $"Chi tiết: {p.Name}";
            ViewBag.Images = await _context.VSImages.AsNoTracking()
                .Where(i => i.ContentId == id && i.FunctionId == 2)
                .Select(i => new { i.FileURL, i.Title })
                .ToListAsync();

            // Load NhaCungUng for Tab 1
            if (p.NCUId.HasValue)
            {
                ViewBag.NhaCungUng = await _context.NhaCungUngs.AsNoTracking()
                    .Where(n => n.CungUngId == p.NCUId.Value)
                    .Select(n => new { n.FullName, n.DiaChi, n.Phone, n.Email, n.Website, n.NguoiDaiDien, n.ChucVu })
                    .FirstOrDefaultAsync();
            }

            // Load XuatXu name
            if (p.XuatXuId.HasValue)
            {
                ViewBag.XuatXuName = await _context.Set<TPApp.Entities.XuatXu>().AsNoTracking()
                    .Where(x => x.Id == p.XuatXuId.Value)
                    .Select(x => x.Title)
                    .FirstOrDefaultAsync();
            }

            // Load LinhVuc list for category name resolution
            ViewBag.LinhVucList = ToSelectList(await _masterService.GetLinhVucAsync(), "");

            return View(GetDetailViewName(p.ProductType), p);
        }

        // ─────────────────────────────────────────
        // GET NhaCungUng JSON (for auto-fill)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetNhaCungUng(int id)
        {
            var ncu = await _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.CungUngId == id)
                .Select(n => new
                {
                    n.FullName,
                    n.DiaChi,
                    n.Phone,
                    n.Email,
                    n.Website,
                    n.NguoiDaiDien,
                    n.ChucVu
                })
                .FirstOrDefaultAsync();

            if (ncu == null) return NotFound();
            return Json(ncu);
        }

        // ─────────────────────────────────────────
        // CREATE (GET)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create(int? productType)
        {
            var pt = productType ?? TypeCongNghe;

            // Auto-generate Code
            var prefix = pt switch { TypeThietBi => "TB", TypeSanPhamTriTue => "TT", _ => "CN" };
            var maxId = await _context.SanPhamCNTBs
                .Where(x => x.ProductType == pt)
                .MaxAsync(x => (int?)x.ID) ?? 0;

            var vm = new SanPhamCNTBFormVm
            {
                ProductType = pt,
                Code = $"{prefix}-{(maxId + 1):D5}",
                StatusId = 1,       // Nháp (Draft)
                SiteId = GetSiteId()
            };
            ViewData["Title"] = "Thêm sản phẩm CNTB";
            await LoadFormSelectListsAsync();
            return View(GetCreateViewName(vm.ProductType), vm);
        }

        // ─────────────────────────────────────────
        // CREATE (POST)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)] // 50 MB
        [RequestFormLimits(ValueLengthLimit = 52_428_800, MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Create(SanPhamCNTBFormVm model)
        {
            ValidateByProductType(model);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Thêm sản phẩm CNTB";
                await LoadFormSelectListsAsync();
                return View(GetCreateViewName(model.ProductType), model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = MapToEntity(model);
                entity.Created = DateTime.Now;
                entity.SiteId = GetSiteId();
                entity.StatusId = 1; // Nháp (Draft)
                entity.Creator = User.Identity?.Name;

                // Auto-generate Code if empty
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    var prefix = model.ProductType switch { 2 => "TB", 3 => "TT", _ => "CN" };
                    var maxId = await _context.SanPhamCNTBs
                        .Where(x => x.ProductType == model.ProductType)
                        .MaxAsync(x => (int?)x.ID) ?? 0;
                    entity.Code = $"{prefix}-{(maxId + 1):D5}";
                }

                // Auto-generate QueryString (slug) from Name
                entity.QueryString = ToSlug(entity.Name ?? "");

                _context.SanPhamCNTBs.Add(entity);
                await _context.SaveChangesAsync();

                // Sync normalized tables
                await CallSyncSP(entity.ID);

                await WriteLog(1, $"Create SanPhamCNTB: {entity.Name} (ID={entity.ID})");

                await transaction.CommitAsync();

                TempData["Success"] = $"Đã thêm sản phẩm '{entity.Name}' thành công.";
                return RedirectToListByType(model.ProductType);
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Có lỗi xảy ra khi lưu dữ liệu.";
                ViewData["Title"] = "Thêm sản phẩm CNTB";
                await LoadFormSelectListsAsync();
                return View(GetCreateViewName(model.ProductType), model);
            }
        }

        // ─────────────────────────────────────────
        // EDIT (GET)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.SanPhamCNTBs.FindAsync(id);
            if (p == null) return NotFound();

            // Block editing items from different sites
            if (p.SiteId.HasValue && p.SiteId.Value != GetSiteId())
            {
                TempData["Error"] = "Không thể sửa sản phẩm thuộc sàn khác.";
                return RedirectToListByType(p.ProductType);
            }

            var vm = MapToFormVm(p);
            ViewData["Title"] = $"Sửa: {p.Name}";
            await LoadFormSelectListsAsync();
            return View(GetEditViewName(p.ProductType), vm);
        }

        // ─────────────────────────────────────────
        // EDIT (POST)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)] // 50 MB
        [RequestFormLimits(ValueLengthLimit = 52_428_800, MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Edit(SanPhamCNTBFormVm model)
        {
            ValidateByProductType(model);

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Sửa sản phẩm CNTB";
                await LoadFormSelectListsAsync();
                return View(GetEditViewName(model.ProductType), model);
            }

            var entity = await _context.SanPhamCNTBs.FindAsync(model.ID);
            if (entity == null) return NotFound();

            // Block editing items from different sites
            if (entity.SiteId.HasValue && entity.SiteId.Value != GetSiteId())
            {
                TempData["Error"] = "Không thể sửa sản phẩm thuộc sàn khác.";
                return RedirectToListByType(model.ProductType);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                UpdateEntity(entity, model);
                entity.Modified = DateTime.Now;
                entity.Modifier = User.Identity?.Name;
                entity.SiteId = GetSiteId();

                // Always regenerate QueryString (slug) from the updated Name
                entity.QueryString = ToSlug(entity.Name ?? "");

                _context.SanPhamCNTBs.Update(entity);
                await _context.SaveChangesAsync();

                await CallSyncSP(entity.ID);

                await WriteLog(2, $"Update SanPhamCNTB: {entity.Name} (ID={entity.ID})");

                await transaction.CommitAsync();

                TempData["Success"] = $"Đã cập nhật sản phẩm '{entity.Name}' thành công.";
                return RedirectToListByType(model.ProductType);
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Có lỗi xảy ra khi lưu dữ liệu.";
                ViewData["Title"] = "Sửa sản phẩm CNTB";
                await LoadFormSelectListsAsync();
                return View(GetEditViewName(model.ProductType), model);
            }
        }

        // ─────────────────────────────────────────
        // DELETE (POST AJAX)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.SanPhamCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

            // Prevent deleting published items (StatusId = 3 = Xuất bản)
            if (entity.StatusId == 3)
                return Json(new { success = false, message = "Không thể xóa sản phẩm đang xuất bản. Hãy chuyển sang trạng thái khác trước." });

            // Prevent deleting items from different sites
            var currentSiteId = GetSiteId();
            if (entity.SiteId.HasValue && entity.SiteId.Value != currentSiteId)
                return Json(new { success = false, message = "Không thể xóa sản phẩm thuộc sàn khác." });

            try
            {
                // Remove ALL FK-linked child records
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SanPhamCNTBCategory WHERE SanPhamCNTBId = {0}", id);
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SanPhamCNTBMedia WHERE SanPhamCNTBId = {0}", id);
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SanPhamCNTBPrice WHERE SanPhamCNTBId = {0}", id);
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SanPhamCNTBIP WHERE SanPhamCNTBId = {0}", id);
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM SanPhamEmbeddings WHERE SanPhamId = {0}", id);

                // Remove related images
                var images = _context.VSImages.Where(i => i.ContentId == id && i.FunctionId == 2);
                _context.VSImages.RemoveRange(images);

                _context.SanPhamCNTBs.Remove(entity);
                await _context.SaveChangesAsync();

                await WriteLog(3, $"Delete SanPhamCNTB: {entity.Name} (ID={id})");
                return Json(new { success = true, message = "Đã xóa sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ─────────────────────────────────────────
        // QUICK CONFIG (AJAX)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> QuickConfig(int id)
        {
            var p = await _context.SanPhamCNTBs.FindAsync(id);
            if (p == null) return NotFound();

            if (p.SiteId.HasValue && p.SiteId.Value != GetSiteId())
                return BadRequest("Không thể cấu hình sản phẩm thuộc sàn khác.");

            ViewBag.ItemId = p.ID;
            ViewBag.ItemName = p.Name;
            ViewBag.StatusId = p.StatusId;
            ViewBag.ProductType = p.ProductType;
            ViewBag.BEffectiveDate = p.bEffectiveDate ?? DateTime.Now;
            ViewBag.EEffectiveDate = p.eEffectiveDate ?? DateTime.Now.AddYears(3);
            ViewBag.Keywords = p.Keywords;
            ViewBag.Statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => new { s.StatusId, s.Title })
                .ToListAsync();

            return PartialView("_QuickConfigPartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickConfig(int id, int? statusId, int? productType,
            DateTime? bEffectiveDate, DateTime? eEffectiveDate, string? keywords)
        {
            var entity = await _context.SanPhamCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

            if (entity.SiteId.HasValue && entity.SiteId.Value != GetSiteId())
                return Json(new { success = false, message = "Không thể cấu hình sản phẩm thuộc sàn khác." });

            entity.StatusId = statusId;
            if (productType.HasValue && productType.Value is TypeCongNghe or TypeThietBi or TypeSanPhamTriTue)
                entity.ProductType = productType.Value;
            entity.bEffectiveDate = bEffectiveDate;
            entity.eEffectiveDate = eEffectiveDate;
            entity.Keywords = keywords;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();
            await WriteLog(2, $"QuickConfig SanPhamCNTB: {entity.Name} (ID={id})");

            return Json(new { success = true, message = "Đã cập nhật cấu hình thành công." });
        }

        // ─────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────
        private void ValidateByProductType(SanPhamCNTBFormVm model)
        {
            switch (model.ProductType)
            {
                case TypeCongNghe:
                    if (!model.Rating.HasValue)
                        ModelState.AddModelError("Rating", "TRL Level là bắt buộc cho Công nghệ.");
                    break;
                case TypeThietBi:
                    if (!model.Rating.HasValue)
                        ModelState.AddModelError("Rating", "Mức độ phát triển là bắt buộc cho Thiết bị.");
                    break;
                case TypeSanPhamTriTue:
                    // SoBang is no longer required — removed from SanPhamTriTue form
                    break;
            }
        }

        private async Task LoadFormSelectListsAsync()
        {
            ViewBag.XuatXuList = ToSelectList(await _masterService.GetXuatXuAsync(), "--- Chọn xuất xứ ---");
            ViewBag.MucDoList = ToSelectList(await _masterService.GetMucDoAsync(), "--- Chọn mức độ ---");
            ViewBag.LinhVucList = ToSelectList(await _masterService.GetLinhVucAsync(), "--- Chọn lĩnh vực ---");
            ViewBag.RootSiteList = ToSelectList(await _masterService.GetRootSitesAsync(), "--- Chọn site ---");
            ViewBag.StatusList = ToSelectList(await _masterService.GetStatusesAsync(), "--- Chọn trạng thái ---");
            ViewBag.NhaCungUngList = ToSelectList(await _masterService.GetNhaCungUngAsync(), "--- Chọn NCC ---");
        }

        private static List<SelectListItem> ToSelectList(List<LookupDto> items, string defaultText)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem(defaultText, "")
            };
            list.AddRange(items.Select(x => new SelectListItem(x.Title, x.Id.ToString())));
            return list;
        }

        private async Task CallSyncSP(int sanPhamId)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"EXEC sp_SyncSanPhamCNTB_Normalized @SanPhamId = {sanPhamId}");
            }
            catch
            {
                // SP may not exist yet — log and continue
            }
        }

        /// <summary>
        /// Converts a Vietnamese string to a URL-friendly slug (no diacritics, lowercase, hyphen-separated).
        /// </summary>
        internal static string ToSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Normalize to NFD to decompose accented characters
            var normalized = text.Normalize(NormalizationForm.FormD);

            // Remove combining (diacritic) characters
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

            // Replace đ → d (special Vietnamese character not covered by NFD)
            slug = slug.Replace('đ', 'd').Replace('Đ', 'd');

            // Replace any non-alphanumeric by hyphen
            slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");

            // Trim leading/trailing hyphens and collapse multiple hyphens
            slug = slug.Trim('-');
            slug = Regex.Replace(slug, @"-{2,}", "-");

            return slug;
        }

        private string GetCreateViewName(int? productType) => productType switch
        {
            TypeThietBi => "CreateThietBi",
            TypeSanPhamTriTue => "CreateSanPhamTriTue",
            _ => "CreateCongNghe"
        };

        private string GetEditViewName(int? productType) => productType switch
        {
            TypeThietBi => "EditThietBi",
            TypeSanPhamTriTue => "EditSanPhamTriTue",
            _ => "EditCongNghe"
        };

        private string GetDetailViewName(int? productType) => productType switch
        {
            TypeThietBi => "DetailThietBi",
            TypeSanPhamTriTue => "DetailSanPhamTriTue",
            _ => "Detail"
        };

        private IActionResult RedirectToListByType(int? productType) => productType switch
        {
            TypeThietBi => RedirectToAction(nameof(ThietBi)),
            TypeSanPhamTriTue => RedirectToAction(nameof(SanPhamTriTue)),
            _ => RedirectToAction(nameof(CongNghe))
        };

        private static SanPhamCNTB MapToEntity(SanPhamCNTBFormVm m) => new()
    {
        Code = m.Code,
        Name = m.Name,
        MoTa = m.MoTa,
        MoTaNgan = m.MoTaNgan,
        ThongSo = m.ThongSo,
        UuDiem = m.UuDiem,
        OriginalPrice = m.OriginalPrice,
        SellPrice = m.SellPrice,
        Currency = m.Currency,
        CategoryId = m.CategoryId,
        NCUId = m.NCUId,
        StatusId = m.StatusId,
        ProductType = m.ProductType,
        SoBang = m.SoBang,
        NgayCapBang = m.NgayCapBang,
        ThoiHan = m.ThoiHan,
        CoQuanChuTri = m.CoQuanChuTri,
        CoQuanChuQuan = m.CoQuanChuQuan,
        Rating = m.Rating,
        XuatXuId = m.XuatXuId,
        MucDoId = m.MucDoId,
        XuatXu = m.XuatXu,
        GiaiThuong = m.GiaiThuong,
        Keywords = m.Keywords,
        LanguageId = 1,
        SiteId = m.SiteId ?? 1, // overridden in Create/Edit POST
        // Owner fields
        OwnerType = m.OwnerType,
        OwnerEmail = m.OwnerEmail,
        HoTen = m.HoTen,
        DiaChi = m.DiaChi,
        Phone = m.Phone,
        LoaiDeTai = m.LoaiDeTai,
        LoaiDeTaiKhac = m.LoaiDeTaiKhac,
        URL = m.URL,
        IsYoutube = m.IsYoutube,
        QuyTrinhHinhAnh = m.QuyTrinhHinhAnh,
        TRLLevel = m.TRLLevel,
        DevelopmentStage = m.DevelopmentStage,
        TargetCustomer = m.TargetCustomer,
        Khachhang = m.Khachhang,
        CooperationGoal = m.CooperationGoal,
        TransferMethod = m.TransferMethod,
        TransferMethodKhac = m.TransferMethodKhac,
        CooperationType = m.CooperationType,
        ApplicationNumber = m.ApplicationNumber,
        AcceptedDate = m.AcceptedDate,
        GiaBanDuKien = m.GiaBanDuKien,
        ChiPhiPhatSinh = m.ChiPhiPhatSinh,
        BaoHanhHoTro = m.BaoHanhHoTro,
        BrochureUrl = m.BrochureUrl,
ChungNhanISO = m.ChungNhanISO,
ChungNhanQuatest = m.ChungNhanQuatest,
ChungNhanKhac = m.ChungNhanKhac,
ChungNhanKhacText = m.ChungNhanKhacText,
DevelopmentStageValue = m.DevelopmentStageValue,
InvestmentGoal = m.InvestmentGoal,
InvestmentGoalKhac = m.InvestmentGoalKhac
    };
        private static void UpdateEntity(SanPhamCNTB e, SanPhamCNTBFormVm m)
    {
        e.Code = m.Code;
        e.Name = m.Name;
        e.MoTa = m.MoTa;
        e.MoTaNgan = m.MoTaNgan;
        e.ThongSo = m.ThongSo;
        e.UuDiem = m.UuDiem;
        e.OriginalPrice = m.OriginalPrice;
        e.SellPrice = m.SellPrice;
        e.Currency = m.Currency;
        e.CategoryId = m.CategoryId;
        e.NCUId = m.NCUId;
        e.StatusId = m.StatusId;
        e.ProductType = m.ProductType;
        e.SoBang = m.SoBang;
        e.NgayCapBang = m.NgayCapBang;
        e.ThoiHan = m.ThoiHan;
        e.CoQuanChuTri = m.CoQuanChuTri;
        e.CoQuanChuQuan = m.CoQuanChuQuan;
        e.Rating = m.Rating;
        e.XuatXuId = m.XuatXuId;
        e.MucDoId = m.MucDoId;
        e.XuatXu = m.XuatXu;
        e.GiaiThuong = m.GiaiThuong;
        e.Keywords = m.Keywords;
        e.SiteId = m.SiteId ?? e.SiteId;
        // Owner fields
        e.OwnerType = m.OwnerType;
        e.OwnerEmail = m.OwnerEmail;
        e.HoTen = m.HoTen;
        e.DiaChi = m.DiaChi;
        e.Phone = m.Phone;
        e.LoaiDeTai = m.LoaiDeTai;
        e.LoaiDeTaiKhac = m.LoaiDeTaiKhac;
        e.URL = m.URL;
        e.IsYoutube = m.IsYoutube;
        e.QuyTrinhHinhAnh = m.QuyTrinhHinhAnh;
        e.TRLLevel = m.TRLLevel;
        e.DevelopmentStage = m.DevelopmentStage;
        e.TargetCustomer = m.TargetCustomer;
        e.Khachhang = m.Khachhang;
        e.CooperationGoal = m.CooperationGoal;
        e.TransferMethod = m.TransferMethod;
        e.TransferMethodKhac = m.TransferMethodKhac;
        e.CooperationType = m.CooperationType;
        e.ApplicationNumber = m.ApplicationNumber;
        e.AcceptedDate = m.AcceptedDate;
        e.GiaBanDuKien = m.GiaBanDuKien;
        e.ChiPhiPhatSinh = m.ChiPhiPhatSinh;
        e.BaoHanhHoTro = m.BaoHanhHoTro;
        e.BrochureUrl = m.BrochureUrl;
e.ChungNhanISO = m.ChungNhanISO;
e.ChungNhanQuatest = m.ChungNhanQuatest;
e.ChungNhanKhac = m.ChungNhanKhac;
e.ChungNhanKhacText = m.ChungNhanKhacText;
e.DevelopmentStageValue = m.DevelopmentStageValue;
e.InvestmentGoal = m.InvestmentGoal;
e.InvestmentGoalKhac = m.InvestmentGoalKhac;
    }
        private static SanPhamCNTBFormVm MapToFormVm(SanPhamCNTB p) => new()
    {
        ID = p.ID,
        Code = p.Code,
        Name = p.Name,
        MoTa = p.MoTa,
        MoTaNgan = p.MoTaNgan,
        ThongSo = p.ThongSo,
        UuDiem = p.UuDiem,
        OriginalPrice = p.OriginalPrice,
        SellPrice = p.SellPrice,
        Currency = p.Currency,
        CategoryId = p.CategoryId,
        NCUId = p.NCUId,
        StatusId = p.StatusId,
        ProductType = p.ProductType,
        SoBang = p.SoBang,
        NgayCapBang = p.NgayCapBang,
        ThoiHan = p.ThoiHan,
        CoQuanChuTri = p.CoQuanChuTri,
        CoQuanChuQuan = p.CoQuanChuQuan,
        Rating = p.Rating,
        XuatXuId = p.XuatXuId,
        MucDoId = p.MucDoId,
        XuatXu = p.XuatXu,
        GiaiThuong = p.GiaiThuong,
        Keywords = p.Keywords,
        SiteId = p.SiteId,
        // Owner fields
        OwnerType = p.OwnerType,
        OwnerEmail = p.OwnerEmail,
        DiaChi = p.DiaChi,
        HoTen = p.HoTen,
        Phone = p.Phone,
        PhoneOther = p.PhoneOther,
        WebUrl = p.WebUrl,
        LoaiDeTai = p.LoaiDeTai,
        LoaiDeTaiKhac = p.LoaiDeTaiKhac,
        URL = p.URL,
        IsYoutube = p.IsYoutube ?? false,
        QuyTrinhHinhAnh = p.QuyTrinhHinhAnh,
        TRLLevel = p.TRLLevel,
        DevelopmentStage = p.DevelopmentStage,
        TargetCustomer = p.TargetCustomer,
        Khachhang = p.Khachhang,
        CooperationGoal = p.CooperationGoal,
        TransferMethod = p.TransferMethod,
        TransferMethodKhac = p.TransferMethodKhac,
        CooperationType = p.CooperationType,
        ApplicationNumber = p.ApplicationNumber,
        AcceptedDate = p.AcceptedDate,
        GiaBanDuKien = p.GiaBanDuKien,
        ChiPhiPhatSinh = p.ChiPhiPhatSinh,
        BaoHanhHoTro = p.BaoHanhHoTro,
        BrochureUrl = p.BrochureUrl,
ChungNhanISO = p.ChungNhanISO ?? false,
ChungNhanQuatest = p.ChungNhanQuatest ?? false,
ChungNhanKhac = p.ChungNhanKhac ?? false,
ChungNhanKhacText = p.ChungNhanKhacText,
DevelopmentStageValue = p.DevelopmentStageValue,
InvestmentGoal = p.InvestmentGoal,
InvestmentGoalKhac = p.InvestmentGoalKhac
    };
    }

    // ─── View Models ───

    public class SanPhamCNTBListItem
    {
        public int ID { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public string? Creator { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int Viewed { get; set; }
        public int ProductType { get; set; }
        public string? SoBang { get; set; }
        public string? NhaCungUngName { get; set; }
        public string? ImageUrl { get; set; }
        public int? SiteId { get; set; }
        public string? PublicUrl { get; set; }
    }

    public class SanPhamCNTBFormVm
    {
        public int ID { get; set; }

        public string? Code { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        public string? Name { get; set; }

        public string? MoTa { get; set; }
        public string? MoTaNgan { get; set; }
        public string? ThongSo { get; set; }
        public string? UuDiem { get; set; }
        public double? OriginalPrice { get; set; }
        public double? SellPrice { get; set; }
        public string? Currency { get; set; }
        public string? CategoryId { get; set; }
        public int? NCUId { get; set; }
        public int? StatusId { get; set; }
        public int ProductType { get; set; } = 1;

        // Sản phẩm trí tuệ
        public string? SoBang { get; set; }
        public DateTime? NgayCapBang { get; set; }
        public DateTime? ThoiHan { get; set; }
        public string? CoQuanChuTri { get; set; }
        public string? CoQuanChuQuan { get; set; }

        // Công nghệ
        public int? Rating { get; set; }

        // Lookup IDs
        public int? XuatXuId { get; set; }
        public int? MucDoId { get; set; }
        public int? SiteId { get; set; }

        public string? XuatXu { get; set; }
        public string? GiaiThuong { get; set; }
        public string? Keywords { get; set; }

        // ── Tab 1: Chủ sở hữu ──
        public int? OwnerType { get; set; }
        public string? OwnerEmail { get; set; }
        public string? DiaChi { get; set; }
        public string? HoTen { get; set; }
        public string? Phone { get; set; }
        public string? PhoneOther { get; set; }
        public string? WebUrl { get; set; }
        public int? LoaiDeTai { get; set; }
        public string? LoaiDeTaiKhac { get; set; }

        // ── Tab 2: Định danh ──
        public string? URL { get; set; }
        public bool IsYoutube { get; set; }
        public string? ApplicationNumber { get; set; }
        public DateTime? AcceptedDate { get; set; }

        // ── Tab 3: Mô tả ──
        public string? QuyTrinhHinhAnh { get; set; }

        // ── Tab 4: TRL ──
        public int? TRLLevel { get; set; }
        public string? DevelopmentStage { get; set; }

        // ── Tab 5: Ứng dụng ──
        public string? TargetCustomer { get; set; }
        public string? Khachhang { get; set; }
        public string? CooperationGoal { get; set; }

        // ── Tab 6: Chuyển giao ──
        public string? TransferMethod { get; set; }
        public string? TransferMethodKhac { get; set; }
        public string? CooperationType { get; set; }
        public string? GiaBanDuKien { get; set; }
        public string? ChiPhiPhatSinh { get; set; }
        public string? BaoHanhHoTro { get; set; }

        // ── Tab 7: Chứng nhận & Tài liệu số ──
        public string? BrochureUrl { get; set; }
        public bool ChungNhanISO { get; set; }
        public bool ChungNhanQuatest { get; set; }
        public bool ChungNhanKhac { get; set; }
        public string? ChungNhanKhacText { get; set; }

        // ── SanPhamTriTue dedicated fields ──
        public int? DevelopmentStageValue { get; set; }
        public string? InvestmentGoal { get; set; }
        public string? InvestmentGoalKhac { get; set; }
    }
}
