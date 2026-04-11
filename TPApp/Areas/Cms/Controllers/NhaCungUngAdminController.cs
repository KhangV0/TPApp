using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Controllers;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;
using TPApp.Services;

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
        public string? PublicUrl { get; set; }
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
        public string Domain { get; set; } = "abc.com";
        public int? SiteId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? Created { get; set; }

        // New fields
        public string? TenVietTat { get; set; }
        public string? LoaiHinhToChuc { get; set; }
        public string? MaSoThue { get; set; }
        public string? Logo { get; set; }
        public string? VideoUrl { get; set; }
        public string? ChungNhan { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize]
    public class NhaCungUngAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICntbMasterService _masterService;
        private readonly IExcelExportService _excelExport;

        public NhaCungUngAdminController(AppDbContext context, IConfiguration configuration, ICntbMasterService masterService, IExcelExportService excelExport)
        {
            _context = context;
            _configuration = configuration;
            _masterService = masterService;
            _excelExport = excelExport;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private string GetDomain() =>
            _configuration["AppSettings:Domain"] ?? "abc.com";

        private const int LogFunctionId = 20; // NhaCungUng

        private async Task WriteLog(int eventId, string content)
        {
            _context.Logs.Add(new Log
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

        // ── INDEX ──
        public async Task<IActionResult> Index(
            string? keyword, int? statusId, bool? isActivated,
            string? linhVuc, string? createdBy, int? siteId, string? dichVu,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var configSiteId = GetSiteId();

            var query = _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.SiteId == null || n.SiteId == configSiteId);

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(n => n.FullName != null && n.FullName.Contains(keyword));
            if (statusId.HasValue)
                query = query.Where(n => n.StatusId == statusId.Value);
            if (isActivated.HasValue)
                query = query.Where(n => n.IsActivated == isActivated.Value);
            if (!string.IsNullOrWhiteSpace(linhVuc))
                query = query.Where(n => n.LinhVucId != null && n.LinhVucId.Contains(linhVuc));
            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(n => n.CreatedBy != null && n.CreatedBy.Contains(createdBy));
            if (siteId.HasValue)
                query = query.Where(n => n.SiteId == siteId.Value);
            if (!string.IsNullOrWhiteSpace(dichVu))
                query = query.Where(n => n.DichVu != null && n.DichVu.Contains(dichVu));
            if (createdFrom.HasValue)
                query = query.Where(n => n.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(n => n.Created <= createdTo.Value.AddDays(1));

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
            ViewBag.LinhVuc = linhVuc;
            ViewBag.CreatedBy = createdBy;
            ViewBag.SiteId = siteId;
            ViewBag.DichVu = dichVu;
            ViewBag.CreatedFrom = createdFrom?.ToString("yyyy-MM-dd");
            ViewBag.CreatedTo = createdTo?.ToString("yyyy-MM-dd");
            ViewBag.Statuses = statuses;
            ViewBag.LinhVucs = await _masterService.GetLinhVucAsync();
            ViewBag.DichVuList = await _masterService.GetDichVuAsync();
            ViewBag.CurrentSiteId = configSiteId;
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        // ── EXPORT EXCEL ──
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? keyword, int? statusId, bool? isActivated,
            string? linhVuc, string? createdBy, int? siteId, string? dichVu,
            DateTime? createdFrom, DateTime? createdTo)
        {
            var configSiteId = GetSiteId();
            var query = _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.SiteId == null || n.SiteId == configSiteId);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(n => n.FullName != null && n.FullName.Contains(keyword));
            if (statusId.HasValue)
                query = query.Where(n => n.StatusId == statusId.Value);
            if (isActivated.HasValue)
                query = query.Where(n => n.IsActivated == isActivated.Value);
            if (!string.IsNullOrWhiteSpace(linhVuc))
                query = query.Where(n => n.LinhVucId != null && n.LinhVucId.Contains(linhVuc));
            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(n => n.CreatedBy != null && n.CreatedBy.Contains(createdBy));
            if (siteId.HasValue)
                query = query.Where(n => n.SiteId == siteId.Value);
            if (!string.IsNullOrWhiteSpace(dichVu))
                query = query.Where(n => n.DichVu != null && n.DichVu.Contains(dichVu));
            if (createdFrom.HasValue)
                query = query.Where(n => n.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(n => n.Created <= createdTo.Value.AddDays(1));

            query = query.OrderByDescending(n => n.Created);

            var statuses = await _context.Statuses.AsNoTracking()
                .ToDictionaryAsync(s => s.StatusId, s => s.Title);

            var items = await query
                .Select(n => new NhaCungUngListItem
                {
                    CungUngId = n.CungUngId,
                    FullName = n.FullName,
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

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            foreach (var item in items)
            {
                if (item.StatusId.HasValue && statuses.TryGetValue(item.StatusId.Value, out var t))
                    item.StatusTitle = t;
                var slug = ProductController.MakeURLFriendly(item.FullName);
                item.PublicUrl = $"{baseUrl}/nha-cung-ung/{slug}-{item.CungUngId}.html";
            }

            return _excelExport.Export(items, $"NhaCungUng_{DateTime.Now:yyyyMMdd}");
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
            ValidateForm(vm);

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

            await WriteLog(1, $"Create NhaCungUng: {vm.FullName} (ID={entity.CungUngId})");

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
            ValidateForm(vm);

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
            entity.TenVietTat = vm.TenVietTat;
            entity.LoaiHinhToChuc = vm.LoaiHinhToChuc;
            entity.MaSoThue = vm.MaSoThue;
            entity.Logo = vm.Logo;
            entity.VideoUrl = vm.VideoUrl;
            entity.ChungNhan = vm.ChungNhan;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();

            await WriteLog(2, $"Update NhaCungUng: {vm.FullName} (ID={vm.CungUngId})");

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

            await WriteLog(3, $"Delete NhaCungUng: {entity.FullName} (ID={id})");

            return Json(new { success = true, message = "Đã xóa nhà cung ứng #" + id });
        }

        // ── HELPERS ──
        private void ValidateForm(NhaCungUngFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.FullName)) ModelState.AddModelError("FullName", "Tên đơn vị là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.LoaiHinhToChuc)) ModelState.AddModelError("LoaiHinhToChuc", "Loại hình tổ chức là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.DiaChi)) ModelState.AddModelError("DiaChi", "Địa chỉ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.Phone)) ModelState.AddModelError("Phone", "Điện thoại là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.Email)) ModelState.AddModelError("Email", "Email chính là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.NguoiDaiDien)) ModelState.AddModelError("NguoiDaiDien", "Người đại diện pháp luật là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.ChucVu)) ModelState.AddModelError("ChucVu", "Chức vụ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.LinhVucId)) ModelState.AddModelError("LinhVucId", "Lĩnh vực hoạt động chính là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.ChucNangChinh)) ModelState.AddModelError("ChucNangChinh", "Chức năng nhiệm vụ / Giá trị cốt lõi là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.DichVu)) ModelState.AddModelError("DichVu", "Dịch vụ khoa học và công nghệ là bắt buộc.");
        }

        private async Task LoadFormSelectListsAsync()
        {
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "Title");

            var linhVucItems = await _masterService.GetLinhVucAsync();
            ViewBag.LinhVucList = linhVucItems
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
                .ToList();

            var dichVuItems = await _masterService.GetDichVuAsync();
            ViewBag.DichVuList = dichVuItems
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
                .ToList();
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
            SiteId = vm.SiteId,
            TenVietTat = vm.TenVietTat,
            LoaiHinhToChuc = vm.LoaiHinhToChuc,
            MaSoThue = vm.MaSoThue,
            Logo = vm.Logo,
            VideoUrl = vm.VideoUrl,
            ChungNhan = vm.ChungNhan
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
            Created = e.Created,
            TenVietTat = e.TenVietTat,
            LoaiHinhToChuc = e.LoaiHinhToChuc,
            MaSoThue = e.MaSoThue,
            Logo = e.Logo,
            VideoUrl = e.VideoUrl,
            ChungNhan = e.ChungNhan
        };
    }
}
