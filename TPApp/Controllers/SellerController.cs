using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class SellerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IESignGateway _eSignGateway;

        public SellerController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IESignGateway eSignGateway)
        {
            _context = context;
            _userManager = userManager;
            _eSignGateway = eSignGateway;
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

        // Helper method to log access
        private async Task LogAccessAsync(int projectId, string action, string? additionalData = null)
        {
            var userId = GetCurrentUserId();
            var log = new ProjectAccessLog
            {
                ProjectId = projectId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                AdditionalData = additionalData
            };
            _context.ProjectAccessLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // GET: /Seller/InvitedProjects
        [HttpGet]
        public async Task<IActionResult> InvitedProjects()
        {
            var userId = GetCurrentUserId();

            // Get all active invitations for this seller
            var invitations = await _context.RFQInvitations
                .Where(i => i.SellerId == userId && i.IsActive)
                .Include(i => i.Project)
                .Include(i => i.RFQRequest)
                .OrderByDescending(i => i.InvitedDate)
                .ToListAsync();

            // Build view model with additional status information
            var viewModel = new List<ViewModel.SellerInvitationVm>();

            foreach (var invitation in invitations)
            {
                if (invitation.Project == null || invitation.RFQRequest == null)
                    continue;

                // Check NDA status
                var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(invitation.ProjectId, userId);

                // Check if proposal already submitted
                var proposalSubmitted = await _context.ProposalSubmissions
                    .AnyAsync(p => p.ProjectId == invitation.ProjectId && p.NguoiTao == userId);

                // Check if deadline passed
                var deadlinePassed = invitation.RFQRequest.HanChotNopHoSo < DateTime.Now;

                viewModel.Add(new ViewModel.SellerInvitationVm
                {
                    Invitation = invitation,
                    Project = invitation.Project,
                    RFQ = invitation.RFQRequest,
                    NdaSigned = ndaSigned,
                    ProposalSubmitted = proposalSubmitted,
                    DeadlinePassed = deadlinePassed,
                    DaysUntilDeadline = (invitation.RFQRequest.HanChotNopHoSo - DateTime.Now).Days
                });
            }

            return View(viewModel);
        }

        // POST: /Seller/AcceptInvitation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvitation(int id)
        {
            var userId = GetCurrentUserId();

            var invitation = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.Id == id && i.SellerId == userId && i.IsActive);

            if (invitation == null)
            {
                return NotFound("Invitation not found");
            }

            // Update invitation status to Accepted
            invitation.StatusId = 2; // Accepted
            invitation.ResponseDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Log the acceptance
            await LogAccessAsync(invitation.ProjectId, "AcceptInvitation", 
                $"InvitationId: {id}");

            TempData["SuccessMessage"] = "Bạn đã chấp nhận lời mời thành công!";

            return RedirectToAction("Details", "Project", new { id = invitation.ProjectId });
        }

        // POST: /Seller/DeclineInvitation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineInvitation(int id, string? reason)
        {
            var userId = GetCurrentUserId();

            var invitation = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.Id == id && i.SellerId == userId && i.IsActive);

            if (invitation == null)
            {
                return NotFound("Invitation not found");
            }

            // Update invitation status to Declined
            invitation.StatusId = 3; // Declined
            invitation.ResponseDate = DateTime.Now;
            invitation.Notes = reason;

            await _context.SaveChangesAsync();

            // Log the decline
            await LogAccessAsync(invitation.ProjectId, "DeclineInvitation",
                $"InvitationId: {id}, Reason: {reason}");

            TempData["InfoMessage"] = "Bạn đã từ chối lời mời.";

            return RedirectToAction("InvitedProjects");
        }
    }
}
