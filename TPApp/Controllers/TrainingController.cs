using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public TrainingController(AppDbContext context, UserManager<ApplicationUser> userManager,
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

        // POST: /Training/SaveInline (AJAX from Step 11)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(
            int projectId, string NoiDungDaoTao,
            int? SoNguoiThamGia, int? SoGioDaoTao,
            DateTime? NgayBatDau, DateTime? NgayKetThuc,
            string? DaHoanThanh, string? VideoYoutubeUrl,
            IFormFile? TaiLieuFile, IFormFile? VideoFile)
        {
            try
            {
                var userId = GetCurrentUserId();

                var entity = await _context.TrainingHandovers
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);

                bool isNew = entity == null;
                if (isNew)
                {
                    entity = new TrainingHandover
                    {
                        ProjectId = projectId,
                        NguoiTao = userId,
                        NgayTao = DateTime.Now
                    };
                }

                entity.NoiDungDaoTao = NoiDungDaoTao;
                entity.SoNguoiThamGia = SoNguoiThamGia;
                entity.SoGioDaoTao = SoGioDaoTao;
                entity.NgayBatDau = NgayBatDau;
                entity.NgayKetThuc = NgayKetThuc;
                entity.DaHoanThanh = DaHoanThanh == "true";
                entity.StatusId = 1;
                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;

                // Handle Tai Lieu file
                if (TaiLieuFile != null && TaiLieuFile.Length > 0)
                {
                    if (TaiLieuFile.Length > 10 * 1024 * 1024)
                        return Json(new { success = false, message = "Tài liệu không được quá 10MB." });

                    var folder = Path.Combine(_environment.WebRootPath, "uploads", "training");
                    Directory.CreateDirectory(folder);
                    var ext = Path.GetExtension(TaiLieuFile.FileName).ToLowerInvariant();
                    var name = $"{Guid.NewGuid()}{ext}";
                    using (var stream = new FileStream(Path.Combine(folder, name), FileMode.Create))
                        await TaiLieuFile.CopyToAsync(stream);
                    entity.TaiLieuDaoTao = $"/uploads/training/{name}";
                }

                // Handle Video: YouTube URL takes priority over file upload
                if (!string.IsNullOrWhiteSpace(VideoYoutubeUrl))
                {
                    entity.VideoHuongDan = VideoYoutubeUrl.Trim();
                }
                else if (VideoFile != null && VideoFile.Length > 0)
                {
                    if (VideoFile.Length > 50 * 1024 * 1024)
                        return Json(new { success = false, message = "Video không được quá 50MB." });

                    var folder = Path.Combine(_environment.WebRootPath, "uploads", "training");
                    Directory.CreateDirectory(folder);
                    var ext = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
                    var name = $"{Guid.NewGuid()}{ext}";
                    using (var stream = new FileStream(Path.Combine(folder, name), FileMode.Create))
                        await VideoFile.CopyToAsync(stream);
                    entity.VideoHuongDan = $"/uploads/training/{name}";
                }

                if (isNew)
                    _context.TrainingHandovers.Add(entity);

                await _context.SaveChangesAsync();

                if (entity.ProjectId.HasValue)
                    await _workflowService.CompleteStep(entity.ProjectId.Value, 11);

                // Notify project members
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    var msg = $"Đào tạo và chuyển giao đã được {(isNew ? "tạo" : "cập nhật")}";
                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId, "🎓 Bước 11: Đào tạo", msg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId, "🎓 Bước 11: Đào tạo", msg);
                }

                return Json(new { success = true, message = isNew ? "✅ Đã lưu đào tạo." : "✅ Đã cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
