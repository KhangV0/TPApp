using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class PilotTestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public PilotTestController(AppDbContext context, UserManager<ApplicationUser> userManager,
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

        // POST: /PilotTest/SaveInline (AJAX from Step 9)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(
            int projectId, string MoTaThuNghiem, string KetQuaThuNghiem,
            string? VanDePhatSinh, string? GiaiPhap,
            DateTime? NgayBatDau, DateTime? NgayKetThuc,
            string? DatYeuCau, IFormFile? FileKetQua)
        {
            try
            {
                var userId = GetCurrentUserId();

                var entity = await _context.PilotTestReports
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);

                bool isNew = entity == null;
                if (isNew)
                {
                    entity = new PilotTestReport
                    {
                        ProjectId = projectId,
                        NguoiTao = userId,
                        NgayTao = DateTime.Now
                    };
                }

                entity.MoTaThuNghiem = MoTaThuNghiem;
                entity.KetQuaThuNghiem = KetQuaThuNghiem;
                entity.VanDePhatSinh = VanDePhatSinh;
                entity.GiaiPhap = GiaiPhap;
                entity.NgayBatDau = NgayBatDau;
                entity.NgayKetThuc = NgayKetThuc;
                entity.DatYeuCau = DatYeuCau == "true";
                entity.StatusId = 1;
                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;

                // Handle file upload
                if (FileKetQua != null && FileKetQua.Length > 0)
                {
                    var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
                    var ext = Path.GetExtension(FileKetQua.FileName).ToLowerInvariant();
                    if (!allowedExt.Contains(ext))
                        return Json(new { success = false, message = "Định dạng file không được hỗ trợ." });

                    if (FileKetQua.Length > 10 * 1024 * 1024)
                        return Json(new { success = false, message = "File không được quá 10MB." });

                    var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "pilot-tests");
                    Directory.CreateDirectory(uploadFolder);

                    var uniqueName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadFolder, uniqueName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileKetQua.CopyToAsync(stream);
                    }
                    entity.FileKetQua = $"/uploads/pilot-tests/{uniqueName}";
                }

                if (isNew)
                    _context.PilotTestReports.Add(entity);

                await _context.SaveChangesAsync();

                // Complete Step 9
                if (entity.ProjectId.HasValue)
                    await _workflowService.CompleteStep(entity.ProjectId.Value, 9);

                // Notify project members
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    var msg = $"Thử nghiệm Pilot đã được {(isNew ? "tạo" : "cập nhật")}. Kết quả: {(entity.DatYeuCau ? "Đạt" : "Chưa đạt")}";
                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId, "🧪 Bước 9: Pilot Test", msg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId, "🧪 Bước 9: Pilot Test", msg);
                }

                return Json(new { success = true, message = isNew ? "✅ Đã lưu thử nghiệm Pilot." : "✅ Đã cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
