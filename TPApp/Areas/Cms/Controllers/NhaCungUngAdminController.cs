using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Areas.Cms.Controllers
{
    // ── DTOs ──
    public class NhaCungUngListItem
    {
        public int CungUngId { get; set; }
        public string? FullName { get; set; }
        public string? HinhDaiDien { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? NguoiDaiDien { get; set; }
        public string? DiaChi { get; set; }
        public bool? IsActivated { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? Created { get; set; }
    }

    public class NhaCungUngFormVm
    {
        public int CungUngId { get; set; }
        public string? FullName { get; set; }
        public string? QueryString { get; set; }
        public string? HinhDaiDien { get; set; }
        public string? DiaChi { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Fax { get; set; }
        public string? Website { get; set; }
        public string? NguoiDaiDien { get; set; }
        public string? ChucVu { get; set; }
        public string? LinhVucId { get; set; }
        public string? ChucNangChinh { get; set; }
        public string? DichVu { get; set; }
        public string? SanPham { get; set; }
        public bool IsActivated { get; set; }
        public int? StatusId { get; set; }
        public int? Rating { get; set; }
        public int? ParentId { get; set; }
        public int? LanguageId { get; set; }
        public string? Keywords { get; set; }
        public string Domain { get; set; } = string.Empty;
        public int? SiteId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? Created { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize]
    public class NhaCungUngAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public NhaCungUngAdminController(AppDbContext context, IConfiguration configuration)
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
            string? keyword, int? statusId, bool? isActivated,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var siteId = GetSiteId();

            var query = _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.SiteId == null || n.SiteId == siteId);

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(n => n.FullName != null && n.FullName.Contains(keyword));
            if (statusId.HasValue)
                query = query.Where(n => n.StatusId == statusId.Value);
            if (isActivated.HasValue)
                query = query.Where(n => n.IsActivated == isActivated.Value);

            // Sort
            query = sortBy?.ToLower() switch
            {
                "fullname" => sortDir == "desc" ? query.OrderByDescending(n => n.FullName) : query.OrderBy(n => n.FullName),
                "phone" => sortDir == "desc" ? query.OrderByDescending(n => n.Phone) : query.OrderBy(n => n.Phone),
                "email" => sortDir == "desc" ? query.OrderByDescending(n => n.Email) : query.OrderBy(n => n.Email),
                "created" => sortDir == "desc" ? query.OrderByDescending(n => n.Created) : query.OrderBy(n => n.Created),
                _ => query.OrderByDescending(n => n.Created)
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
                .Select(n => new NhaCungUngListItem
                {
                    CungUngId = n.CungUngId,
                    FullName = n.FullName,
                    HinhDaiDien = n.HinhDaiDien,
                    Phone = n.Phone,
                    Email = n.Email,
                    NguoiDaiDien = n.NguoiDaiDien,
                    DiaChi = n.DiaChi,
                    IsActivated = n.IsActivated,
                    StatusId = n.StatusId,
                    CreatedBy = n.CreatedBy,
                    Created = n.Created
                })
                .ToListAsync();

            // Map status titles
            foreach (var item in items)
            {
                if (item.StatusId.HasValue && statusDict.TryGetValue(item.StatusId.Value, out var sTitle))
                    item.StatusTitle = sTitle;
            }

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.IsActivated = isActivated;
            ViewBag.Statuses = statuses;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        // ── CREATE GET ──
        public async Task<IActionResult> Create()
        {
            var vm = new NhaCungUngFormVm
            {
                LanguageId = 1,
                Domain = GetDomain(),
                SiteId = GetSiteId(),
                StatusId = 1,
                IsActivated = false
            };
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ── CREATE POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhaCungUngFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.FullName))
                ModelState.AddModelError("FullName", "Tên nhà cung ứng không được để trống.");

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = MapToEntity(vm);
            entity.Created = DateTime.Now;
            entity.CreatedBy = User.Identity?.Name;

            _context.NhaCungUngs.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo nhà cung ứng thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ──
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.NhaCungUngs.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToVm(entity);
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ── EDIT POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NhaCungUngFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.FullName))
                ModelState.AddModelError("FullName", "Tên nhà cung ứng không được để trống.");

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = await _context.NhaCungUngs.FindAsync(vm.CungUngId);
            if (entity == null) return NotFound();

            entity.FullName = vm.FullName;
            entity.QueryString = vm.QueryString;
            entity.HinhDaiDien = vm.HinhDaiDien;
            entity.DiaChi = vm.DiaChi;
            entity.Phone = vm.Phone;
            entity.Email = vm.Email;
            entity.Fax = vm.Fax;
            entity.Website = vm.Website;
            entity.NguoiDaiDien = vm.NguoiDaiDien;
            entity.ChucVu = vm.ChucVu;
            entity.LinhVucId = vm.LinhVucId;
            entity.ChucNangChinh = vm.ChucNangChinh;
            entity.DichVu = vm.DichVu;
            entity.SanPham = vm.SanPham;
            entity.IsActivated = vm.IsActivated;
            entity.StatusId = vm.StatusId;
            entity.Rating = vm.Rating;
            entity.ParentId = vm.ParentId;
            entity.Keywords = vm.Keywords;
            entity.SiteId = vm.SiteId;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật nhà cung ứng thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.NhaCungUngs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy nhà cung ứng." });

            if (entity.IsActivated == true)
                return Json(new { success = false, message = "Không thể xóa nhà cung ứng đang kích hoạt." });

            _context.NhaCungUngs.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa nhà cung ứng #" + id });
        }

        // ── HELPERS ──
        private async Task LoadFormSelectListsAsync()
        {
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "Title");
        }

        private NhaCungUng MapToEntity(NhaCungUngFormVm vm) => new()
        {
            FullName = vm.FullName,
            QueryString = vm.QueryString,
            HinhDaiDien = vm.HinhDaiDien,
            DiaChi = vm.DiaChi,
            Phone = vm.Phone,
            Email = vm.Email,
            Fax = vm.Fax,
            Website = vm.Website,
            NguoiDaiDien = vm.NguoiDaiDien,
            ChucVu = vm.ChucVu,
            LinhVucId = vm.LinhVucId,
            ChucNangChinh = vm.ChucNangChinh,
            DichVu = vm.DichVu,
            SanPham = vm.SanPham,
            IsActivated = vm.IsActivated,
            StatusId = vm.StatusId,
            Rating = vm.Rating,
            ParentId = vm.ParentId,
            LanguageId = vm.LanguageId,
            Keywords = vm.Keywords,
            Domain = vm.Domain,
            SiteId = vm.SiteId
        };

        private NhaCungUngFormVm MapToVm(NhaCungUng e) => new()
        {
            CungUngId = e.CungUngId,
            FullName = e.FullName,
            QueryString = e.QueryString,
            HinhDaiDien = e.HinhDaiDien,
            DiaChi = e.DiaChi,
            Phone = e.Phone,
            Email = e.Email,
            Fax = e.Fax,
            Website = e.Website,
            NguoiDaiDien = e.NguoiDaiDien,
            ChucVu = e.ChucVu,
            LinhVucId = e.LinhVucId,
            ChucNangChinh = e.ChucNangChinh,
            DichVu = e.DichVu,
            SanPham = e.SanPham,
            IsActivated = e.IsActivated ?? false,
            StatusId = e.StatusId,
            Rating = e.Rating,
            ParentId = e.ParentId,
            LanguageId = e.LanguageId,
            Keywords = e.Keywords,
            Domain = e.Domain,
            SiteId = e.SiteId,
            CreatedBy = e.CreatedBy,
            Created = e.Created
        };
    }
}
