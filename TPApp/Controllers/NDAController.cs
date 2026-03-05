using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class NDAController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public NDAController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService, Services.INotificationQueueService notifQueue)
        {
            _context = context;
            _userManager = userManager;
            _workflowService = workflowService;
            _notifQueue = notifQueue;
        }

        // Helper method to get current user ID as int
        private int GetCurrentUserId()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }

        // GET: /NDA/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 2)
            if (!await _workflowService.CanAccessStep(projectId.Value, 2)) return Forbid();

            var existing = await _context.NDAAgreements.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            // Pre-fill
            var user = await _userManager.GetUserAsync(User);
            var model = new NDAAgreement
            {
                ProjectId = projectId,
                BenA = user?.FullName ?? "",
                BenB = "Trung tâm Thông tin, Thống kê và Ứng dụng tiến bộ khoa học công nghệ",
                LoaiNDA = "Mẫu chuẩn của Sàn",
                ThoiHanBaoMat = "3 năm"
            };

            return View(model);
        }

        // POST: /NDA/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NDAAgreement model)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (!model.DaDongY)
            {
                ModelState.AddModelError("DaDongY", "Bạn phải đồng ý điều khoản trước khi tiếp tục.");
            }

            if (ModelState.IsValid)
            {
                // Set metadata
                model.NguoiTao = userId;
                model.NgayTao = DateTime.Now;
                model.StatusId = 1;

                _context.NDAAgreements.Add(model);
                await _context.SaveChangesAsync();

                // Complete Step 2
                await _workflowService.CompleteStep(model.ProjectId.Value, 2);

                // Notify buyer: NDA signed, proceed to Step 3
                await _notifQueue.QueueAsync(userId, model.ProjectId,
                    "NDA đã hoàn tất",
                    "Thỏa thuận bảo mật đã ký thành công. Tiến hành bước 3: Tạo yêu cầu báo giá (RFQ).");

                return RedirectToAction("Details", "Project", new { id = model.ProjectId });
            }

            return View(model);
        }

        // POST: /NDA/AgreeNda (AJAX - simplified Step 2, no OTP)
        [HttpPost]
        public async Task<IActionResult> AgreeNda([FromBody] AgreeNdaRequest request)
        {
            try
            {
                if (request.ProjectId <= 0)
                    return Json(new { success = false, message = "Project ID không hợp lệ." });

                var userId = GetCurrentUserId();
                var isMember = await _context.ProjectMembers
                    .AnyAsync(m => m.ProjectId == request.ProjectId && m.UserId == userId);
                if (!isMember)
                    return Json(new { success = false, message = "Bạn không có quyền truy cập dự án này." });

                // Check if already agreed
                var existing = await _context.NDAAgreements
                    .FirstOrDefaultAsync(x => x.ProjectId == request.ProjectId);
                if (existing != null && existing.DaDongY)
                    return Json(new { success = true, message = "Đã đồng ý trước đó." });

                if (existing != null)
                {
                    // Update existing
                    existing.DaDongY = true;
                    existing.NgaySua = DateTime.Now;
                    existing.StatusId = 1;
                }
                else
                {
                    // Create new
                    existing = new NDAAgreement
                    {
                        ProjectId = request.ProjectId,
                        BenA = request.BenA ?? "",
                        BenB = request.BenB ?? "Trung tâm Thông tin, Thống kê và Ứng dụng tiến bộ khoa học công nghệ",
                        LoaiNDA = "Mẫu chuẩn của Sàn",
                        ThoiHanBaoMat = "12 tháng",
                        DaDongY = true,
                        NguoiTao = userId,
                        NgayTao = DateTime.Now,
                        StatusId = 1
                    };
                    _context.NDAAgreements.Add(existing);
                }

                await _context.SaveChangesAsync();

                // Complete Step 2
                await _workflowService.CompleteStep(request.ProjectId, 2);

                // Notify
                await _notifQueue.QueueAsync(userId, request.ProjectId,
                    "NDA đã hoàn tất",
                    "Thỏa thuận bảo mật đã được đồng ý. Tiến hành bước 3: Tạo yêu cầu báo giá (RFQ).");

                return Json(new { success = true, message = "Đã đồng ý thỏa thuận bảo mật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public class AgreeNdaRequest
        {
            public int ProjectId { get; set; }
            public string? BenA { get; set; }
            public string? BenB { get; set; }
        }
    }
}
