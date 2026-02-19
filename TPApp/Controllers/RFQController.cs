using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class RFQController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public RFQController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService, Services.INotificationQueueService notifQueue)
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

        // GET: /RFQ/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
             if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 3)
            if (!await _workflowService.CanAccessStep(projectId.Value, 3)) return Forbid();

            var existing = await _context.RFQRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new RFQRequest { ProjectId = projectId });
        }

        // POST: /RFQ/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RFQRequest model)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (ModelState.IsValid)
            {
                // Set metadata
                model.NguoiTao = userId;
                model.NgayTao = DateTime.Now;
                model.StatusId = 1;
                model.DaGuiNhaCungUng = false;

                _context.RFQRequests.Add(model);
                await _context.SaveChangesAsync();

                // Complete Step 3
                await _workflowService.CompleteStep(model.ProjectId.Value, 3);

                // Notify buyer: RFQ created
                await _notifQueue.QueueAsync(userId, model.ProjectId,
                    "RFQ đã tạo",
                    "Yêu cầu báo giá đã tạo thành công. Hãy mời nhà cung ứng nộp hồ sơ.");

                return RedirectToAction("Details", "Project", new { id = model.ProjectId });
            }

            return View(model);
        }

        // GET: /RFQ/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rfq = await _context.RFQRequests
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rfq == null)
            {
                return NotFound();
            }

            // Optional: Check if user is owner or admin
            // var userId = _userManager.GetUserId(User);
            // if (rfq.NguoiTao != userId && !User.IsInRole("Admin")) return Forbid();

            return View(rfq);
        }

        // POST: /RFQ/SendToSuppliers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToSuppliers(int id, int[] selectedSellerIds)
        {
            var rfq = await _context.RFQRequests.FindAsync(id);
            if (rfq == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == rfq.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Create invitation records for each selected seller
            if (selectedSellerIds != null && selectedSellerIds.Length > 0)
            {
                foreach (var sellerId in selectedSellerIds)
                {
                    // Check if invitation already exists
                    var existingInvitation = await _context.RFQInvitations
                        .FirstOrDefaultAsync(i => i.ProjectId == rfq.ProjectId && 
                                                 i.RFQId == id && 
                                                 i.SellerId == sellerId);

                    if (existingInvitation == null)
                    {
                        var invitation = new Entities.RFQInvitation
                        {
                            ProjectId = rfq.ProjectId.Value,
                            RFQId = id,
                            SellerId = sellerId,
                            InvitedDate = DateTime.Now,
                            StatusId = 0, // Invited
                            IsActive = true
                        };
                        _context.RFQInvitations.Add(invitation);

                        // Notify seller: invited to submit proposal (no projectId — seller has no access yet)
                        await _notifQueue.QueueAsync(sellerId, null,
                            "📨 Bạn được mời nộp hồ sơ báo giá",
                            $"Bạn được mời nộp hồ sơ báo giá. Hãy vào mục 'Dự án được mời' để xem chi tiết và nộp hồ sơ trước hạn.");
                    }
                    else if (!existingInvitation.IsActive)
                    {
                        // Reactivate existing invitation
                        existingInvitation.IsActive = true;
                        existingInvitation.InvitedDate = DateTime.Now;
                        existingInvitation.StatusId = 0; // Reset to Invited
                    }
                }
            }

            // Don't set DaGuiNhaCungUng flag - allow multiple sends
            // rfq.DaGuiNhaCungUng = true;
            rfq.StatusId = 2; // Sent status
            _context.Update(rfq);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã gửi RFQ đến {selectedSellerIds?.Length ?? 0} nhà cung ứng.";

            return RedirectToAction("Details", "Project", new { id = rfq.ProjectId });
        }
    }
}
