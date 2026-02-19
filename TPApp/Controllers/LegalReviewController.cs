using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class LegalReviewController : Controller
    {
        private readonly AppDbContext         _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment  _environment;
        private readonly ILegalReviewService  _legalReviewService;
        private readonly ILogger<LegalReviewController> _logger;

        public LegalReviewController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILegalReviewService legalReviewService,
            ILogger<LegalReviewController> logger)
        {
            _context           = context;
            _userManager       = userManager;
            _environment       = environment;
            _legalReviewService = legalReviewService;
            _logger            = logger;
        }

        private int GetCurrentUserId()
        {
            var s = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(s) || !int.TryParse(s, out int id))
                throw new UnauthorizedAccessException("Invalid user ID");
            return id;
        }

        private async Task<(bool canAccess, bool isBuyer, bool isSeller)> GetAccessAsync(int projectId, int userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return (false, false, false);
            bool isBuyer  = project.CreatedBy == userId;
            bool isSeller = project.SelectedSellerId == userId;
            return (isBuyer || isSeller, isBuyer, isSeller);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /LegalReview/Details?projectId=
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(int? projectId)
        {
            if (projectId == null) return NotFound();
            var userId = GetCurrentUserId();
            var (canAccess, _, _) = await GetAccessAsync(projectId.Value, userId);
            if (!canAccess) return Forbid();

            var form = await _context.LegalReviewForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);

            // If not yet created (Step 5 not done), return informational view
            ViewBag.ProjectId = projectId;
            ViewBag.Form      = form;
            ViewBag.Comments  = form != null
                ? await _context.ContractComments
                    .Where(c => c.LegalReviewFormId == form.Id)
                    .OrderBy(c => c.NgayTao)
                    .ToListAsync()
                : new List<ContractComment>();

            return View(form);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /LegalReview/Approve  (AJAX)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Approve([FromBody] LegalReviewActionDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, isBuyer, _) = await GetAccessAsync(dto.ProjectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });
                if (!isBuyer)   return Json(new { success = false, message = "Chỉ Buyer có quyền phê duyệt." });

                bool ok = await _legalReviewService.ApproveAsync(dto.ProjectId, userId);
                return Json(ok
                    ? new { success = true,  message = "✅ Hợp đồng đã được phê duyệt! Bước 7 đã mở." }
                    : new { success = false, message = "Không tìm thấy hồ sơ pháp lý." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving contract for project {Id}", dto.ProjectId);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /LegalReview/Reject  (AJAX)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Reject([FromBody] LegalReviewRejectDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, isBuyer, _) = await GetAccessAsync(dto.ProjectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });
                if (!isBuyer)   return Json(new { success = false, message = "Chỉ Buyer có quyền từ chối." });

                if (string.IsNullOrWhiteSpace(dto.Reason))
                    return Json(new { success = false, message = "Vui lòng nhập lý do từ chối." });

                bool ok = await _legalReviewService.RejectAsync(dto.ProjectId, userId, dto.Reason);
                return Json(ok
                    ? new { success = true,  message = "🔄 Đã yêu cầu chỉnh sửa hợp đồng." }
                    : new { success = false, message = "Không tìm thấy hồ sơ pháp lý." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting contract for project {Id}", dto.ProjectId);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /LegalReview/AddComment  (AJAX)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, _, _) = await GetAccessAsync(dto.ProjectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });

                if (string.IsNullOrWhiteSpace(dto.Text))
                    return Json(new { success = false, message = "Nội dung comment không được để trống." });

                var user = await _context.Users.FindAsync(userId);
                var authorName = user?.FullName ?? user?.UserName ?? "Người dùng";

                var commentType = Enum.TryParse<ContractCommentType>(dto.Type, out var ct)
                    ? ct : ContractCommentType.General;

                var comment = await _legalReviewService.AddCommentAsync(
                    dto.ProjectId, userId, authorName, dto.Text, commentType);

                if (comment == null)
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ pháp lý." });

                return Json(new
                {
                    success    = true,
                    message    = "Comment đã được thêm.",
                    commentId  = comment.Id,
                    authorName = comment.AuthorName,
                    text       = comment.CommentText,
                    type       = comment.CommentType,
                    time       = comment.NgayTao.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment for project {Id}", dto.ProjectId);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /LegalReview/ResolveComment  (AJAX)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ResolveComment([FromBody] ResolveCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                bool ok = await _legalReviewService.ResolveCommentAsync(dto.CommentId, userId);
                return Json(ok
                    ? new { success = true,  message = "Comment đã được đánh dấu đã xử lý." }
                    : new { success = false, message = "Không tìm thấy comment." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving comment {Id}", dto.CommentId);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /LegalReview/UploadRevision  (AJAX)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadRevision(int projectId, IFormFile? revisionFile)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, _, _) = await GetAccessAsync(projectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });

                if (revisionFile == null || revisionFile.Length == 0)
                    return Json(new { success = false, message = "Vui lòng chọn file." });

                bool ok = await _legalReviewService.UploadRevisionAsync(projectId, userId, revisionFile, _environment);
                return Json(ok
                    ? new { success = true,  message = "📎 Phiên bản hợp đồng mới đã được tải lên." }
                    : new { success = false, message = "File không hợp lệ hoặc không tìm thấy hồ sơ." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading revision for project {Id}", projectId);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }

    // ─── DTOs ────────────────────────────────────────────────────────────────
    public class LegalReviewActionDto  { public int ProjectId { get; set; } }
    public class LegalReviewRejectDto  { public int ProjectId { get; set; } public string Reason { get; set; } = ""; }
    public class AddCommentDto         { public int ProjectId { get; set; } public string Text { get; set; } = ""; public string Type { get; set; } = "General"; }
    public class ResolveCommentDto     { public int CommentId { get; set; } }
}
