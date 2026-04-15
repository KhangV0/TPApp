using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Areas.Cms.Controllers;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class DangKyNhaCungUngController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICntbMasterService _masterService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DangKyNhaCungUngController(
            AppDbContext context,
            IConfiguration configuration,
            ICntbMasterService masterService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _masterService = masterService;
            _userManager = userManager;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private string GetDomain() =>
            _configuration["AppSettings:Domain"] ?? "abc.com";

        private async Task<(int userId, NhaCungUng? existing)> GetCurrentUserAndEntityAsync()
        {
            var userIdStr = _userManager.GetUserId(User);
            int.TryParse(userIdStr, out var userId);
            var existing = await _context.NhaCungUngs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.UserId == userId);
            return (userId, existing);
        }

        // ── GET ──
        [HttpGet]
        public async Task<IActionResult> DangKy()
        {
            var (userId, existing) = await GetCurrentUserAndEntityAsync();

            NhaCungUngFormVm vm;
            if (existing != null)
            {
                // Đã có hồ sơ → load lên để chỉnh sửa
                vm = MapToVm(existing);
                ViewBag.IsUpdate = true;

                // Lấy tên trạng thái
                if (existing.StatusId.HasValue)
                {
                    var status = await _context.Statuses.FindAsync(existing.StatusId.Value);
                    ViewBag.StatusTitle = status?.Title ?? "Không xác định";
                    ViewBag.StatusId    = existing.StatusId.Value;
                }
            }
            else
            {
                // Chưa có → form trống
                vm = new NhaCungUngFormVm
                {
                    LanguageId = 1,
                    Domain     = GetDomain(),
                    SiteId     = GetSiteId(),
                    StatusId   = 1,
                    IsActivated = false
                };
                ViewBag.IsUpdate = false;
            }

            await LoadSelectListsAsync();
            return View(vm);
        }

        // ── POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(NhaCungUngFormVm vm)
        {
            ValidateForm(vm);

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                ViewBag.IsUpdate = vm.CungUngId > 0;
                if (vm.CungUngId > 0 && vm.StatusId.HasValue)
                    ViewBag.StatusId = vm.StatusId.Value;
                return View(vm);
            }

            var userIdStr = _userManager.GetUserId(User);
            int.TryParse(userIdStr, out var userId);

            if (vm.CungUngId > 0)
            {
                // ── CẬP NHẬT ──
                var entity = await _context.NhaCungUngs.FindAsync(vm.CungUngId);
                if (entity == null || entity.UserId != userId) return Forbid();

                entity.FullName       = vm.FullName;
                entity.TenVietTat     = vm.TenVietTat;
                entity.LoaiHinhToChuc = vm.LoaiHinhToChuc;
                entity.DiaChi         = vm.DiaChi;
                entity.Phone          = vm.Phone;
                entity.Email          = vm.Email;
                entity.Fax            = vm.Fax;
                entity.Website        = vm.Website;
                entity.MaSoThue       = vm.MaSoThue;
                entity.NguoiDaiDien   = vm.NguoiDaiDien;
                entity.ChucVu         = vm.ChucVu;
                entity.LinhVucId      = vm.LinhVucId;
                entity.ChucNangChinh  = vm.ChucNangChinh;
                entity.DichVu         = vm.DichVu;
                entity.SanPham        = vm.SanPham;
                entity.Logo           = vm.Logo;
                entity.HinhDaiDien    = vm.HinhDaiDien;
                entity.VideoUrl       = vm.VideoUrl;
                entity.ChungNhan      = vm.ChungNhan;
                entity.Keywords       = vm.Keywords;
                entity.Modified       = DateTime.Now;
                entity.Modifier       = User.Identity?.Name;
                entity.StatusId       = 2; // Reset về "Đang duyệt" để admin xét lại

                await _context.SaveChangesAsync();
                TempData["DangKySuccess"] = "Cập nhật hồ sơ thành công! Hồ sơ đang chờ Ban quản trị xét duyệt lại.";
            }
            else
            {
                // ── TẠO MỚI ──
                var entity = new NhaCungUng
                {
                    FullName       = vm.FullName,
                    TenVietTat     = vm.TenVietTat,
                    LoaiHinhToChuc = vm.LoaiHinhToChuc,
                    DiaChi         = vm.DiaChi,
                    Phone          = vm.Phone,
                    Email          = vm.Email,
                    Fax            = vm.Fax,
                    Website        = vm.Website,
                    MaSoThue       = vm.MaSoThue,
                    NguoiDaiDien   = vm.NguoiDaiDien,
                    ChucVu         = vm.ChucVu,
                    LinhVucId      = vm.LinhVucId,
                    ChucNangChinh  = vm.ChucNangChinh,
                    DichVu         = vm.DichVu,
                    SanPham        = vm.SanPham,
                    Logo           = vm.Logo,
                    HinhDaiDien    = vm.HinhDaiDien,
                    VideoUrl       = vm.VideoUrl,
                    ChungNhan      = vm.ChungNhan,
                    Keywords       = vm.Keywords,
                    IsActivated    = false,
                    StatusId       = 1,
                    LanguageId     = 1,
                    SiteId         = GetSiteId(),
                    Domain         = GetDomain(),
                    UserId         = userId,
                    Created        = DateTime.Now,
                    CreatedBy      = User.Identity?.Name
                };

                _context.NhaCungUngs.Add(entity);
                await _context.SaveChangesAsync();
                TempData["DangKySuccess"] = "Đăng ký thành công! Hồ sơ đang chờ Ban quản trị xét duyệt.";
            }

            return RedirectToAction(nameof(DangKy));
        }

        // ── Helpers ──
        private void ValidateForm(NhaCungUngFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.FullName))       ModelState.AddModelError("FullName",       "Tên đơn vị là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.LoaiHinhToChuc)) ModelState.AddModelError("LoaiHinhToChuc", "Loại hình tổ chức là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.DiaChi))         ModelState.AddModelError("DiaChi",         "Địa chỉ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.Phone))          ModelState.AddModelError("Phone",          "Số điện thoại là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.Email))          ModelState.AddModelError("Email",          "Email là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.NguoiDaiDien))   ModelState.AddModelError("NguoiDaiDien",   "Người đại diện pháp luật là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.ChucVu))         ModelState.AddModelError("ChucVu",         "Chức vụ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.LinhVucId))      ModelState.AddModelError("LinhVucId",      "Lĩnh vực hoạt động là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.ChucNangChinh))  ModelState.AddModelError("ChucNangChinh",  "Chức năng nhiệm vụ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.DichVu))         ModelState.AddModelError("DichVu",         "Dịch vụ KH&CN là bắt buộc.");
        }

        private async Task LoadSelectListsAsync()
        {
            var linhVuc = await _masterService.GetLinhVucAsync();
            ViewBag.LinhVucList = linhVuc
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
                .ToList();

            var dichVu = await _masterService.GetDichVuAsync();
            ViewBag.DichVuList = dichVu
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
                .ToList();
        }

        private static NhaCungUngFormVm MapToVm(NhaCungUng e) => new()
        {
            CungUngId      = e.CungUngId,
            FullName       = e.FullName,
            TenVietTat     = e.TenVietTat,
            LoaiHinhToChuc = e.LoaiHinhToChuc,
            DiaChi         = e.DiaChi,
            Phone          = e.Phone,
            Email          = e.Email,
            Fax            = e.Fax,
            Website        = e.Website,
            MaSoThue       = e.MaSoThue,
            NguoiDaiDien   = e.NguoiDaiDien,
            ChucVu         = e.ChucVu,
            LinhVucId      = e.LinhVucId,
            ChucNangChinh  = e.ChucNangChinh,
            DichVu         = e.DichVu,
            SanPham        = e.SanPham,
            Logo           = e.Logo,
            HinhDaiDien    = e.HinhDaiDien,
            VideoUrl       = e.VideoUrl,
            ChungNhan      = e.ChungNhan,
            Keywords       = e.Keywords,
            IsActivated    = e.IsActivated ?? false,
            StatusId       = e.StatusId,
            LanguageId     = e.LanguageId,
            Domain         = e.Domain,
            SiteId         = e.SiteId,
            QueryString    = e.QueryString,
        };
    }
}
