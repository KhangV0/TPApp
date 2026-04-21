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
    public class DangKyTuVanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICntbMasterService _masterService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DangKyTuVanController(
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

        private async Task<(int userId, NhaTuVan? existing)> GetCurrentUserAndEntityAsync()
        {
            var userIdStr = _userManager.GetUserId(User);
            int.TryParse(userIdStr, out var userId);
            var existing = await _context.NhaTuVans
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.UserId == userId);
            return (userId, existing);
        }

        // ── GET ──
        [HttpGet]
        public async Task<IActionResult> DangKy()
        {
            var (userId, existing) = await GetCurrentUserAndEntityAsync();

            NhaTuVanFormVm vm;
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
                    ViewBag.StatusId = existing.StatusId.Value;
                }
            }
            else
            {
                // Chưa có → form trống, pre-fill từ user profile
                var user = await _userManager.GetUserAsync(User);
                vm = new NhaTuVanFormVm
                {
                    FullName = user?.FullName,
                    Email = user?.Email,
                    Phone = user?.PhoneNumber,
                    LanguageId = 1,
                    Domain = GetDomain(),
                    SiteId = GetSiteId(),
                    StatusId = 1,
                    IsActivated = false
                };
                ViewBag.IsUpdate = false;
            }

            await LoadSelectListsAsync();
            return View(vm);
        }

        // ── POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(NhaTuVanFormVm vm)
        {
            ValidateForm(vm);

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                ViewBag.IsUpdate = vm.TuVanId > 0;
                if (vm.TuVanId > 0 && vm.StatusId.HasValue)
                    ViewBag.StatusId = vm.StatusId.Value;
                return View(vm);
            }

            var userIdStr = _userManager.GetUserId(User);
            int.TryParse(userIdStr, out var userId);

            if (vm.TuVanId > 0)
            {
                // ── CẬP NHẬT ──
                var entity = await _context.NhaTuVans.FindAsync(vm.TuVanId);
                if (entity == null || entity.UserId != userId) return Forbid();

                entity.FullName         = vm.FullName ?? "";
                entity.Email            = vm.Email;
                entity.DateOfBirth      = vm.DateOfBirth ?? "";
                entity.HinhDaiDien      = vm.HinhDaiDien;
                entity.DiaChi           = vm.DiaChi ?? "";
                entity.Phone            = vm.Phone ?? "";
                entity.HocHam           = vm.HocHam;
                entity.CoQuan           = vm.CoQuan;
                entity.ChucVu           = vm.ChucVu;
                entity.LinhVucId        = vm.LinhVucId;
                entity.DichVu           = vm.DichVu;
                entity.KetQuaNghienCuu  = vm.KetQuaNghienCuu;
                entity.Keywords         = vm.Keywords;
                entity.MaDinhDanh       = vm.MaDinhDanh;
                entity.TongTrichDan     = vm.TongTrichDan;
                entity.HIndex           = vm.HIndex;
                entity.QuaTrinhDaoTao   = vm.QuaTrinhDaoTao;
                entity.QuaTrinhCongTac  = vm.QuaTrinhCongTac;
                entity.CongBoKhoaHoc   = vm.CongBoKhoaHoc;
                entity.SangChe          = vm.SangChe;
                entity.DuAnNghienCuu    = vm.DuAnNghienCuu;
                entity.KinhNghiem       = vm.KinhNghiem;
                entity.HoSoDinhKem      = vm.HoSoDinhKem;
                entity.HiepHoiKhoaHoc   = vm.HiepHoiKhoaHoc;
                entity.Modified         = DateTime.Now;
                entity.Modifier         = User.Identity?.Name;
                entity.StatusId         = 2; // Reset về "Đang duyệt"

                await _context.SaveChangesAsync();
                TempData["DangKySuccess"] = "Cập nhật hồ sơ thành công! Hồ sơ đang chờ Ban quản trị xét duyệt lại.";
            }
            else
            {
                // ── TẠO MỚI ──
                var entity = new NhaTuVan
                {
                    FullName         = vm.FullName ?? "",
                    Email            = vm.Email,
                    DateOfBirth      = vm.DateOfBirth ?? "",
                    HinhDaiDien      = vm.HinhDaiDien,
                    DiaChi           = vm.DiaChi ?? "",
                    Phone            = vm.Phone ?? "",
                    HocHam           = vm.HocHam,
                    CoQuan           = vm.CoQuan,
                    ChucVu           = vm.ChucVu,
                    LinhVucId        = vm.LinhVucId,
                    DichVu           = vm.DichVu,
                    KetQuaNghienCuu  = vm.KetQuaNghienCuu,
                    Keywords         = vm.Keywords,
                    MaDinhDanh       = vm.MaDinhDanh,
                    TongTrichDan     = vm.TongTrichDan,
                    HIndex           = vm.HIndex,
                    QuaTrinhDaoTao   = vm.QuaTrinhDaoTao,
                    QuaTrinhCongTac  = vm.QuaTrinhCongTac,
                    CongBoKhoaHoc   = vm.CongBoKhoaHoc,
                    SangChe          = vm.SangChe,
                    DuAnNghienCuu    = vm.DuAnNghienCuu,
                    KinhNghiem       = vm.KinhNghiem,
                    HoSoDinhKem      = vm.HoSoDinhKem,
                    HiepHoiKhoaHoc   = vm.HiepHoiKhoaHoc,
                    IsActivated      = false,
                    StatusId         = 1,
                    LanguageId       = 1,
                    SiteId           = GetSiteId(),
                    Domain           = GetDomain(),
                    UserId           = userId,
                    Created          = DateTime.Now,
                    CreatedBy        = User.Identity?.Name
                };

                _context.NhaTuVans.Add(entity);
                await _context.SaveChangesAsync();
                TempData["DangKySuccess"] = "Đăng ký thành công! Hồ sơ đang chờ Ban quản trị xét duyệt.";
            }

            return RedirectToAction(nameof(DangKy));
        }

        // ── Helpers ──
        private void ValidateForm(NhaTuVanFormVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.FullName))        ModelState.AddModelError("FullName",        "Họ tên là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.HocHam))          ModelState.AddModelError("HocHam",          "Học hàm / Học vị là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.CoQuan))          ModelState.AddModelError("CoQuan",          "Cơ quan công tác là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.ChucVu))          ModelState.AddModelError("ChucVu",          "Chức vụ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.LinhVucId))       ModelState.AddModelError("LinhVucId",       "Lĩnh vực nghiên cứu là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.DichVu))          ModelState.AddModelError("DichVu",          "Dịch vụ tư vấn chuyên sâu là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.DiaChi))          ModelState.AddModelError("DiaChi",          "Địa chỉ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.Phone))           ModelState.AddModelError("Phone",           "Số điện thoại là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.QuaTrinhDaoTao))  ModelState.AddModelError("QuaTrinhDaoTao",  "Quá trình đào tạo là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.QuaTrinhCongTac)) ModelState.AddModelError("QuaTrinhCongTac", "Quá trình công tác là bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.CongBoKhoaHoc))   ModelState.AddModelError("CongBoKhoaHoc",   "Bài báo khoa học là bắt buộc.");
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

        private static NhaTuVanFormVm MapToVm(NhaTuVan e) => new()
        {
            TuVanId          = e.TuVanId,
            FullName         = e.FullName,
            QueryString      = e.QueryString,
            Email            = e.Email,
            DateOfBirth      = e.DateOfBirth,
            HinhDaiDien      = e.HinhDaiDien,
            DiaChi           = e.DiaChi,
            Phone            = e.Phone,
            HocHam           = e.HocHam,
            CoQuan           = e.CoQuan,
            ChucVu           = e.ChucVu,
            LinhVucId        = e.LinhVucId,
            DichVu           = e.DichVu,
            KetQuaNghienCuu  = e.KetQuaNghienCuu,
            IsActivated      = e.IsActivated ?? false,
            StatusId         = e.StatusId,
            Rating           = e.Rating,
            ParentId         = e.ParentId,
            LanguageId       = e.LanguageId,
            Keywords         = e.Keywords,
            Domain           = e.Domain,
            SiteId           = e.SiteId,
            CreatedBy        = e.CreatedBy,
            Created          = e.Created,
            MaDinhDanh       = e.MaDinhDanh,
            TongTrichDan     = e.TongTrichDan,
            HIndex           = e.HIndex,
            QuaTrinhDaoTao   = e.QuaTrinhDaoTao,
            QuaTrinhCongTac  = e.QuaTrinhCongTac,
            CongBoKhoaHoc   = e.CongBoKhoaHoc,
            SangChe          = e.SangChe,
            DuAnNghienCuu    = e.DuAnNghienCuu,
            KinhNghiem       = e.KinhNghiem,
            HoSoDinhKem      = e.HoSoDinhKem,
            HiepHoiKhoaHoc   = e.HiepHoiKhoaHoc
        };
    }
}
