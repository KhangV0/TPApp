using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class LegalReviewService : ILegalReviewService
    {
        private readonly AppDbContext _context;
        private readonly INotificationQueueService _notifQueue;
        private readonly ILogger<LegalReviewService> _logger;

        public LegalReviewService(
            AppDbContext context,
            INotificationQueueService notifQueue,
            ILogger<LegalReviewService> logger)
        {
            _context  = context;
            _notifQueue = notifQueue;
            _logger   = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Auto-create Contract Draft v1 from Negotiation data
        // ─────────────────────────────────────────────────────────────────────
        public async Task<LegalReviewForm?> AutoCreateDraftAsync(int projectId)
        {
            var existing = await _context.LegalReviewForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);

            if (existing != null)
            {
                _logger.LogInformation("LegalReviewForm already exists for project {Id}, skipping auto-create.", projectId);
                return existing;
            }

            var negotiation = await _context.NegotiationForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return null;

            var html = BuildHtmlSnapshot(negotiation, project);

            var draft = new LegalReviewForm
            {
                ProjectId         = projectId,
                NegotiationFormId = negotiation?.Id,
                Version           = 1,
                HtmlSnapshot      = html,
                NguoiKiemTra     = "Hệ thống",
                KetQuaKiemTra    = "Bản nháp hợp đồng tự động từ kết quả thương lượng.",
                StatusId          = (int)LegalReviewStatus.Draft,
                ReviewDeadline    = DateTime.Now.AddDays(7),
                NguoiTao          = project.CreatedBy,
                NgayTao           = DateTime.Now
            };

            _context.LegalReviewForms.Add(draft);
            await _context.SaveChangesAsync();

            // Notify buyer + seller
            await NotifyPartiesAsync(projectId, project,
                "📄 Hợp đồng nháp đã tạo",
                "Bản nháp hợp đồng đã được tạo tự động sau khi hoàn tất thương lượng. Vui lòng kiểm tra bước 6.");

            _logger.LogInformation("Auto-created LegalReviewForm v1 for project {Id}.", projectId);
            return draft;
        }

        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> ApproveAsync(int projectId, int userId)
        {
            var form = await _context.LegalReviewForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (form == null) return false;

            form.DaDuyet    = true;
            form.ReviewedBy = userId;
            form.NgayKiemTra = DateTime.Now;
            form.StatusId   = (int)LegalReviewStatus.Completed;
            form.NguoiSua   = userId;
            form.NgaySua    = DateTime.Now;
            form.KetQuaKiemTra = "Đã kiểm tra và thông qua.";

            // Mark Step 6 as Completed
            var step6 = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == 6);
            if (step6 != null)
            {
                step6.StatusId      = (int)StepStatus.Completed;
                step6.CompletedDate = DateTime.Now;
            }

            // Unlock Step 7 (set to InProgress so CanAccessStep returns true)
            var step7 = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == 7);
            if (step7 != null && step7.StatusId == (int)StepStatus.NotStarted)
                step7.StatusId = (int)StepStatus.InProgress;

            await _context.SaveChangesAsync();

            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
                await NotifyPartiesAsync(projectId, project,
                    "✅ Hợp đồng đã được duyệt",
                    "Bản nháp hợp đồng đã được phê duyệt. Bước 7 đã được mở khóa.");

            _logger.LogInformation("LegalReviewForm approved for project {Id} by user {UserId}.", projectId, userId);
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> RejectAsync(int projectId, int userId, string reason)
        {
            var form = await _context.LegalReviewForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (form == null) return false;

            form.DaDuyet         = false;
            form.ReviewedBy      = userId;
            form.NgayKiemTra     = DateTime.Now;
            form.StatusId        = (int)LegalReviewStatus.ChangesRequested;
            form.RejectionReason = reason;
            form.NguoiSua        = userId;
            form.NgaySua         = DateTime.Now;

            await _context.SaveChangesAsync();

            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
                await NotifyPartiesAsync(projectId, project,
                    "🔄 Hợp đồng cần chỉnh sửa",
                    $"Bản nháp hợp đồng cần được chỉnh sửa: {reason}");

            _logger.LogInformation("LegalReviewForm rejected for project {Id} by user {UserId}.", projectId, userId);
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        public async Task<ContractComment?> AddCommentAsync(int projectId, int userId,
            string authorName, string text, ContractCommentType type)
        {
            var form = await _context.LegalReviewForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (form == null) return null;

            var comment = new ContractComment
            {
                ProjectId         = projectId,
                LegalReviewFormId = form.Id,
                CommentText       = text,
                CommentType       = (int)type,
                AuthorId          = userId,
                AuthorName        = authorName,
                NgayTao           = DateTime.Now
            };

            _context.ContractComments.Add(comment);

            // When first comment added, set status to UnderReview if still Draft
            if (form.StatusId == (int)LegalReviewStatus.Draft)
            {
                form.StatusId = (int)LegalReviewStatus.UnderReview;
                form.NguoiSua = userId;
                form.NgaySua  = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Comment added to project {Id} by user {UserId}.", projectId, userId);
            return comment;
        }

        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> ResolveCommentAsync(int commentId, int userId)
        {
            var comment = await _context.ContractComments.FindAsync(commentId);
            if (comment == null) return false;

            comment.IsResolved = true;
            comment.NgaySua    = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> UploadRevisionAsync(int projectId, int userId,
            IFormFile file, IWebHostEnvironment environment)
        {
            var form = await _context.LegalReviewForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (form == null) return false;

            var allowed = new[] { ".pdf", ".doc", ".docx" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext)) return false;
            if (file.Length > 30 * 1024 * 1024) return false;

            var folder = Path.Combine(environment.WebRootPath, "uploads", "contracts");
            Directory.CreateDirectory(folder);
            var fname = $"project{projectId}_v{form.Version + 1}_{Guid.NewGuid()}{ext}";
            using (var stream = new FileStream(Path.Combine(folder, fname), FileMode.Create))
                await file.CopyToAsync(stream);

            form.ContractFilePath = $"/uploads/contracts/{fname}";
            form.Version++;
            form.StatusId = (int)LegalReviewStatus.UnderReview;
            form.DaDuyet  = false;
            form.RejectionReason = null;
            form.NguoiSua = userId;
            form.NgaySua  = DateTime.Now;

            await _context.SaveChangesAsync();

            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
                await NotifyPartiesAsync(projectId, project,
                    "📎 Phiên bản hợp đồng mới",
                    $"Phiên bản hợp đồng v{form.Version} đã được tải lên. Vui lòng kiểm tra.");

            _logger.LogInformation("Revision v{Ver} uploaded for project {Id} by user {UserId}.", form.Version, projectId, userId);
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> IsCompletedAsync(int projectId)
        {
            var form = await _context.LegalReviewForms
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);
            return form?.StatusId == (int)LegalReviewStatus.Completed;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────
        private static string BuildHtmlSnapshot(NegotiationForm? neg, Project project)
        {
            var price = neg?.GiaChotCuoiCung?.ToString("N0") ?? "Chưa xác định";
            var terms = neg?.DieuKhoanThanhToan ?? "Chưa xác định";
            var date  = DateTime.Now.ToString("dd/MM/yyyy");

            return $@"
<div style='font-family:Arial,sans-serif;max-width:800px;margin:auto;padding:24px;border:1px solid #ccc;border-radius:8px'>
  <h2 style='text-align:center;color:#1a3c6e'>HỢP ĐỒNG THƯƠNG MẠI – BẢN NHÁP</h2>
  <p style='text-align:center;color:#666'>Phiên bản 1 · Ngày tạo: {date}</p>
  <hr/>
  <h4>Điều 1 – Các bên tham gia</h4>
  <p><strong>Bên A (Bên mua):</strong> Được xác định qua hệ thống dự án #{project.Id}</p>
  <p><strong>Bên B (Bên bán):</strong> Nhà cung ứng được chọn theo Bước 4</p>
  <h4>Điều 2 – Giá trị hợp đồng</h4>
  <p>Giá chốt cuối cùng: <strong>{price} VNĐ</strong></p>
  <h4>Điều 3 – Điều khoản thanh toán</h4>
  <pre style='white-space:pre-wrap;background:#f8f9fa;padding:12px;border-radius:4px'>{terms}</pre>
  <h4>Điều 4 – Cam kết thực hiện</h4>
  <p>Hai bên cam kết thực hiện đúng các điều khoản đã thương lượng tại Bước 5.</p>
  <hr/>
  <p style='color:#888;font-size:12px'>Tài liệu này được tạo tự động từ kết quả thương lượng. Vui lòng kiểm tra và chỉnh sửa trước khi ký chính thức.</p>
</div>";
        }

        private async Task NotifyPartiesAsync(int projectId, Project project,
            string title, string message)
        {
            try
            {
                if (project.CreatedBy.HasValue)
                    await _notifQueue.QueueAsync(project.CreatedBy.Value, projectId, title, message);
                if (project.SelectedSellerId.HasValue)
                    await _notifQueue.QueueAsync(project.SelectedSellerId.Value, projectId, title, message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for project {Id}.", projectId);
            }
        }
    }
}
