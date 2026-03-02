using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Added for AnyAsync and FirstOrDefaultAsync
using Newtonsoft.Json;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class HandoverController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public HandoverController(AppDbContext context, UserManager<ApplicationUser> userManager,
            Services.IWorkflowService workflowService, Services.INotificationQueueService notifQueue)
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

        // GET: /Handover/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 9)
            if (!await _workflowService.CanAccessStep(projectId.Value, 9)) return Forbid();

            var existing = await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new HandoverReport { ProjectId = projectId });
        }

        // POST: /Handover/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HandoverReport model, string DanhMucThietBiJson, string DanhMucHoSoJson)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    // Assign JSON strings
                    model.DanhMucThietBiJson = DanhMucThietBiJson;
                    model.DanhMucHoSoJson = DanhMucHoSoJson;

                    // Set Metadata
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.HandoverReports.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 9
                    await _workflowService.CompleteStep(model.ProjectId.Value, 9);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Handover/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.HandoverReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: /Handover/SaveInline (AJAX from Step 10)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(
            int projectId, string? DaHoanThanhDaoTao,
            int? DanhGiaSao, string? NhanXet)
        {
            try
            {
                var userId = GetCurrentUserId();

                var entity = await _context.HandoverReports
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);

                bool isNew = entity == null;
                if (isNew)
                {
                    entity = new HandoverReport
                    {
                        ProjectId = projectId,
                        NguoiTao = userId,
                        NgayTao = DateTime.Now
                    };
                }

                entity.DaHoanThanhDaoTao = DaHoanThanhDaoTao == "true";
                entity.DanhGiaSao = DanhGiaSao;
                entity.NhanXet = NhanXet;
                entity.StatusId = 1;
                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;

                if (isNew)
                    _context.HandoverReports.Add(entity);

                await _context.SaveChangesAsync();

                // Complete Step 10
                if (entity.ProjectId.HasValue)
                    await _workflowService.CompleteStep(entity.ProjectId.Value, 10);

                // Notify project members
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    var msg = $"Bàn giao đã được {(isNew ? "tạo" : "cập nhật")}. Đánh giá: {DanhGiaSao ?? 0} sao";
                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId, "🤝 Bước 10: Bàn giao", msg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId, "🤝 Bước 10: Bàn giao", msg);
                }

                return Json(new { success = true, message = isNew ? "✅ Đã lưu bàn giao." : "✅ Đã cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
