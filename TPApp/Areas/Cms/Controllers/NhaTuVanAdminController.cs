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
    public class NhaTuVanListItem
    {
        public int TuVanId { get; set; }
        public string? FullName { get; set; }
        public string? HinhDaiDien { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? HocHam { get; set; }
        public string? CoQuan { get; set; }
        public string? ChucVu { get; set; }
        public bool? IsActivated { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        public string? PublicUrl { get; set; }
    }

    public class NhaTuVanFormVm
    {
        public int TuVanId { get; set; }
        public string? FullName { get; set; }
        public string? QueryString { get; set; }
        public string? Email { get; set; }
        public string? DateOfBirth { get; set; }
        public string? HinhDaiDien { get; set; }
        public string? DiaChi { get; set; }
        public string? Phone { get; set; }
        public string? HocHam { get; set; }
        public string? CoQuan { get; set; }
        public string? ChucVu { get; set; }
        public string? LinhVucId { get; set; }
        public string? DichVu { get; set; }
        public string? KetQuaNghienCuu { get; set; }
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
        public string? MaDinhDanh { get; set; }
        public int? TongTrichDan { get; set; }
        public int? HIndex { get; set; }
        public string? QuaTrinhDaoTao { get; set; }
        public string? QuaTrinhCongTac { get; set; }
        public string? CongBoKhoaHoc { get; set; }
        public string? SangChe { get; set; }
        public string? DuAnNghienCuu { get; set; }
        public string? KinhNghiem { get; set; }
        public string? HoSoDinhKem { get; set; }
        public string? HiepHoiKhoaHoc { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize]
    public class NhaTuVanAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICntbMasterService _masterService;
        private readonly IExcelExportService _excelExport;

        public NhaTuVanAdminController(AppDbContext context, IConfiguration configuration, ICntbMasterService masterService, IExcelExportService excelExport)
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

        private const int LogFunctionId = 21; // NhaTuVan

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

            var query = _context.NhaTuVans.AsNoTracking()
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
                .Select(n => new NhaTuVanListItem
                {
                    TuVanId = n.TuVanId,
                    FullName = n.FullName,
                    HinhDaiDien = n.HinhDaiDien,
                    Phone = n.Phone,
                    Email = n.Email,
                    HocHam = n.HocHam,
                    CoQuan = n.CoQuan,
                    ChucVu = n.ChucVu,
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
            var query = _context.NhaTuVans.AsNoTracking()
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
                .Select(n => new NhaTuVanListItem
                {
                    TuVanId = n.TuVanId,
                    FullName = n.FullName,
                    Phone = n.Phone,
                    Email = n.Email,
                    HocHam = n.HocHam,
                    CoQuan = n.CoQuan,
                    ChucVu = n.ChucVu,
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
                item.PublicUrl = $"{baseUrl}/nha-tu-van/{slug}-{item.TuVanId}.html";
            }

            return _excelExport.Export(items, $"NhaTuVan_{DateTime.Now:yyyyMMdd}");
        }

        // ── CREATE GET ──
        public async Task<IActionResult> Create()
        {
            var vm = new NhaTuVanFormVm
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
        public async Task<IActionResult> Create(NhaTuVanFormVm vm)
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

            _context.NhaTuVans.Add(entity);
            await _context.SaveChangesAsync();

            await WriteLog(1, $"Create NhaTuVan: {vm.FullName} (ID={entity.TuVanId})");

            TempData["Success"] = "Đã tạo nhà tư vấn thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ──
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.NhaTuVans.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToVm(entity);
            await LoadFormSelectListsAsync();
            return View(vm);
        }

        // ── EDIT POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NhaTuVanFormVm vm)
        {
            ValidateForm(vm);

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync();
                return View(vm);
            }

            var entity = await _context.NhaTuVans.FindAsync(vm.TuVanId);
            if (entity == null) return NotFound();

            entity.FullName = vm.FullName ?? "";
            entity.QueryString = vm.QueryString;
            entity.Email = vm.Email;
            entity.DateOfBirth = vm.DateOfBirth ?? "";
            entity.HinhDaiDien = vm.HinhDaiDien;
            entity.DiaChi = vm.DiaChi ?? "";
            entity.Phone = vm.Phone ?? "";
            entity.HocHam = vm.HocHam;
            entity.CoQuan = vm.CoQuan;
            entity.ChucVu = vm.ChucVu;
            entity.LinhVucId = vm.LinhVucId;
            entity.DichVu = vm.DichVu;
            entity.KetQuaNghienCuu = vm.KetQuaNghienCuu;
            entity.IsActivated = vm.IsActivated;
            entity.StatusId = vm.StatusId;
            entity.Rating = vm.Rating;
            entity.ParentId = vm.ParentId;
            entity.Keywords = vm.Keywords;
            entity.SiteId = vm.SiteId;
            entity.MaDinhDanh = vm.MaDinhDanh;
            entity.TongTrichDan = vm.TongTrichDan;
            entity.HIndex = vm.HIndex;
            entity.QuaTrinhDaoTao = vm.QuaTrinhDaoTao;
            entity.QuaTrinhCongTac = vm.QuaTrinhCongTac;
            entity.CongBoKhoaHoc = vm.CongBoKhoaHoc;
            entity.SangChe = vm.SangChe;
            entity.DuAnNghienCuu = vm.DuAnNghienCuu;
            entity.KinhNghiem = vm.KinhNghiem;
            entity.HoSoDinhKem = vm.HoSoDinhKem;
            entity.HiepHoiKhoaHoc = vm.HiepHoiKhoaHoc;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();

            await WriteLog(2, $"Update NhaTuVan: {vm.FullName} (ID={vm.TuVanId})");

            TempData["Success"] = "Đã cập nhật nhà tư vấn thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.NhaTuVans.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy nhà tư vấn." });

            if (entity.IsActivated == true)
                return Json(new { success = false, message = "Không thể xóa nhà tư vấn đang kích hoạt." });

            _context.NhaTuVans.Remove(entity);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete NhaTuVan: {entity.FullName} (ID={id})");

            return Json(new { success = true, message = "Đã xóa nhà tư vấn #" + id });
        }

        // ── HELPERS ──
        private void ValidateForm(NhaTuVanFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.FullName)) ModelState.AddModelError("FullName", "Họ tên là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.HocHam)) ModelState.AddModelError("HocHam", "Học hàm / Học vị là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.HinhDaiDien)) ModelState.AddModelError("HinhDaiDien", "Ảnh đại diện là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.CoQuan)) ModelState.AddModelError("CoQuan", "Cơ quan công tác là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.ChucVu)) ModelState.AddModelError("ChucVu", "Chức vụ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.LinhVucId)) ModelState.AddModelError("LinhVucId", "Lĩnh vực nghiên cứu là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.QuaTrinhDaoTao)) ModelState.AddModelError("QuaTrinhDaoTao", "Quá trình đào tạo là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.QuaTrinhCongTac)) ModelState.AddModelError("QuaTrinhCongTac", "Quá trình công tác là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.CongBoKhoaHoc)) ModelState.AddModelError("CongBoKhoaHoc", "Bài báo khoa học là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.DichVu)) ModelState.AddModelError("DichVu", "Dịch vụ tư vấn chuyên sâu là bắt buộc.");
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

        private NhaTuVan MapToEntity(NhaTuVanFormVm vm) => new()
        {
            FullName = vm.FullName ?? "",
            QueryString = vm.QueryString,
            Email = vm.Email,
            DateOfBirth = vm.DateOfBirth ?? "",
            HinhDaiDien = vm.HinhDaiDien,
            DiaChi = vm.DiaChi ?? "",
            Phone = vm.Phone ?? "",
            HocHam = vm.HocHam,
            CoQuan = vm.CoQuan,
            ChucVu = vm.ChucVu,
            LinhVucId = vm.LinhVucId,
            DichVu = vm.DichVu,
            KetQuaNghienCuu = vm.KetQuaNghienCuu,
            IsActivated = vm.IsActivated,
            StatusId = vm.StatusId,
            Rating = vm.Rating,
            ParentId = vm.ParentId,
            LanguageId = vm.LanguageId,
            Keywords = vm.Keywords,
            Domain = vm.Domain,
            SiteId = vm.SiteId,
            MaDinhDanh = vm.MaDinhDanh,
            TongTrichDan = vm.TongTrichDan,
            HIndex = vm.HIndex,
            QuaTrinhDaoTao = vm.QuaTrinhDaoTao,
            QuaTrinhCongTac = vm.QuaTrinhCongTac,
            CongBoKhoaHoc = vm.CongBoKhoaHoc,
            SangChe = vm.SangChe,
            DuAnNghienCuu = vm.DuAnNghienCuu,
            KinhNghiem = vm.KinhNghiem,
            HoSoDinhKem = vm.HoSoDinhKem,
            HiepHoiKhoaHoc = vm.HiepHoiKhoaHoc
        };

        private NhaTuVanFormVm MapToVm(NhaTuVan e) => new()
        {
            TuVanId = e.TuVanId,
            FullName = e.FullName,
            QueryString = e.QueryString,
            Email = e.Email,
            DateOfBirth = e.DateOfBirth,
            HinhDaiDien = e.HinhDaiDien,
            DiaChi = e.DiaChi,
            Phone = e.Phone,
            HocHam = e.HocHam,
            CoQuan = e.CoQuan,
            ChucVu = e.ChucVu,
            LinhVucId = e.LinhVucId,
            DichVu = e.DichVu,
            KetQuaNghienCuu = e.KetQuaNghienCuu,
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
            MaDinhDanh = e.MaDinhDanh,
            TongTrichDan = e.TongTrichDan,
            HIndex = e.HIndex,
            QuaTrinhDaoTao = e.QuaTrinhDaoTao,
            QuaTrinhCongTac = e.QuaTrinhCongTac,
            CongBoKhoaHoc = e.CongBoKhoaHoc,
            SangChe = e.SangChe,
            DuAnNghienCuu = e.DuAnNghienCuu,
            KinhNghiem = e.KinhNghiem,
            HoSoDinhKem = e.HoSoDinhKem,
            HiepHoiKhoaHoc = e.HiepHoiKhoaHoc
        };
    }
}
