using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class TechDocController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public TechDocController(AppDbContext context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment, Services.IWorkflowService workflowService,
            Services.INotificationQueueService notifQueue)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _workflowService = workflowService;
            _notifQueue = notifQueue;
        }

        private int GetCurrentUserId()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                throw new UnauthorizedAccessException("Invalid user ID");
            return userId;
        }

        // POST: /TechDoc/SaveInline (AJAX from Step 12)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(
            int projectId, string? SourceCode, string? Database,
            string? GhiChu, DateTime? NgayBanGiao, string? DaBanGiaoDayDu,
            IFormFile? TaiLieuKyThuatFile, IFormFile? TaiLieuHuongDanFile,
            IFormFile? TaiLieuBaoTriFile)
        {
            try
            {
                var userId = GetCurrentUserId();

                var entity = await _context.TechnicalDocHandovers
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);

                bool isNew = entity == null;
                if (isNew)
                {
                    entity = new TechnicalDocHandover
                    {
                        ProjectId = projectId,
                        DanhMucHoSo = "[]", // default empty JSON
                        NguoiTao = userId,
                        NgayTao = DateTime.Now
                    };
                }

                entity.SourceCode = SourceCode;
                entity.Database = Database;
                entity.GhiChu = GhiChu;
                entity.NgayBanGiao = NgayBanGiao;
                entity.DaBanGiaoDayDu = DaBanGiaoDayDu == "true";
                entity.StatusId = 1;
                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;

                // Handle file uploads
                await SaveFileIfPresent(TaiLieuKyThuatFile, f => entity.TaiLieuKyThuat = f, 10);
                await SaveFileIfPresent(TaiLieuHuongDanFile, f => entity.TaiLieuHuongDanSuDung = f, 10);
                await SaveFileIfPresent(TaiLieuBaoTriFile, f => entity.TaiLieuBaoTri = f, 10);

                if (isNew)
                    _context.TechnicalDocHandovers.Add(entity);

                await _context.SaveChangesAsync();

                if (entity.ProjectId.HasValue)
                    await _workflowService.CompleteStep(entity.ProjectId.Value, 12);

                // Notify project members
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    var msg = $"Hồ sơ kỹ thuật đã được {(isNew ? "tạo" : "cập nhật")}. Bàn giao: {(entity.DaBanGiaoDayDu ? "Đầy đủ" : "Chưa đầy đủ")}";
                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId, "📄 Bước 12: Hồ sơ KT", msg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId, "📄 Bước 12: Hồ sơ KT", msg);
                }

                return Json(new { success = true, message = isNew ? "✅ Đã lưu hồ sơ kỹ thuật." : "✅ Đã cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private async Task SaveFileIfPresent(IFormFile? file, Action<string> setter, int maxMB)
        {
            if (file == null || file.Length <= 0) return;
            if (file.Length > maxMB * 1024 * 1024)
                throw new InvalidOperationException($"File {file.FileName} quá {maxMB}MB.");

            var folder = Path.Combine(_environment.WebRootPath, "uploads", "techdoc");
            Directory.CreateDirectory(folder);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var name = $"{Guid.NewGuid()}{ext}";
            using (var stream = new FileStream(Path.Combine(folder, name), FileMode.Create))
                await file.CopyToAsync(stream);
            setter($"/uploads/techdoc/{name}");
        }
    }
}
