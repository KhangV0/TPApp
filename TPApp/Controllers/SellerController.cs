using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    [Authorize]
    public class SellerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IESignGateway _eSignGateway;
        private readonly IInvitationService _invitationService;
        private readonly IProposalService _proposalService;

        public SellerController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IESignGateway eSignGateway,
            IInvitationService invitationService,
            IProposalService proposalService)
        {
            _context = context;
            _userManager = userManager;
            _eSignGateway = eSignGateway;
            _invitationService = invitationService;
            _proposalService = proposalService;
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

            try
            {
                // Use InvitationService with strict guards
                await _invitationService.AcceptInvitationAsync(id, userId);

                // Log the acceptance
                var invitation = await _invitationService.GetInvitationByIdAsync(id);
                if (invitation != null)
                {
                    await LogAccessAsync(invitation.ProjectId, "AcceptInvitation", 
                        $"InvitationId: {id}");
                }

                TempData["SuccessMessage"] = "Bạn đã chấp nhận lời mời thành công!";

                return RedirectToAction("InvitedProjects");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("InvitedProjects");
            }
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

        // GET: /Seller/ProjectDetails/7
        public async Task<IActionResult> ProjectDetails(int id)
        {
            var userId = GetCurrentUserId();

            // Guard 1: Check if user has valid invitation
            var invitation = await _context.RFQInvitations
                .Include(i => i.Project)
                .Include(i => i.RFQRequest)
                .FirstOrDefaultAsync(i => i.ProjectId == id &&
                                         i.SellerId == userId &&
                                         i.IsActive);

            if (invitation == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa được mời tham gia dự án này.";
                return RedirectToAction("InvitedProjects");
            }

            // Guard 2: Check if invitation is declined or expired
            if (invitation.StatusId == 4) // Declined
            {
                TempData["ErrorMessage"] = "Bạn đã từ chối lời mời này.";
                return RedirectToAction("InvitedProjects");
            }

            if (invitation.StatusId == 5) // Expired
            {
                TempData["ErrorMessage"] = "Lời mời đã hết hạn.";
                return RedirectToAction("InvitedProjects");
            }

            // Guard 3: Check NDA signature
            var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(id, userId);
            if (!ndaSigned)
            {
                TempData["WarningMessage"] = "Bạn cần ký NDA trước khi xem chi tiết dự án.";
                return RedirectToAction("SignNda", "Project", new { projectId = id });
            }

            // Update invitation status to Viewed if first time
            if (invitation.StatusId == 0) // Invited
            {
                invitation.StatusId = 1; // Viewed
                invitation.ViewedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            // Log seller access
            await LogAccessAsync(id, "ViewProjectDetails");

            // Get RFQ details
            var rfq = invitation.RFQRequest;

            // Check if seller has submitted proposal
            var proposal = await _context.ProposalSubmissions
                .FirstOrDefaultAsync(p => p.ProjectId == id && p.NguoiTao == userId);

            // Prepare view model
            ViewBag.ProjectId = id;
            ViewBag.ProjectName = invitation.Project?.ProjectName;
            ViewBag.InvitationStatus = invitation.StatusId;
            ViewBag.InvitationStatusName = GetInvitationStatusName(invitation.StatusId);
            ViewBag.NdaSigned = ndaSigned;
            ViewBag.HasProposal = proposal != null;
            ViewBag.ProposalStatus = proposal?.StatusId;
            ViewBag.RfqDeadline = rfq?.HanChotNopHoSo;
            ViewBag.RfqDescription = rfq?.YeuCauKyThuat;

            return View(invitation.Project);
        }

        // Helper: Get invitation status name
        private string GetInvitationStatusName(int statusId)
        {
            return statusId switch
            {
                0 => "Đã mời",
                1 => "Đã xem",
                2 => "Đã chấp nhận",
                3 => "Đã gửi báo giá",
                4 => "Đã từ chối",
                5 => "Đã hết hạn",
                _ => "Không xác định"
            };
        }

        // POST: /Seller/SubmitProposal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitProposal(int projectId, IFormFile? technicalSolution, 
            decimal? preliminaryPrice, string? implementationTime, IFormFile? capabilityProfile)
        {
            var userId = GetCurrentUserId();

            try
            {
                // Validate inputs
                if (technicalSolution == null || technicalSolution.Length == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng tải lên giải pháp kỹ thuật.";
                    return RedirectToAction("ProjectDetails", new { id = projectId });
                }

                if (capabilityProfile == null || capabilityProfile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng tải lên hồ sơ năng lực.";
                    return RedirectToAction("ProjectDetails", new { id = projectId });
                }

                if (string.IsNullOrWhiteSpace(implementationTime))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập thời gian triển khai.";
                    return RedirectToAction("ProjectDetails", new { id = projectId });
                }

                // Check if can submit proposal (5 guards in ProposalService)
                var canSubmit = await _proposalService.CanSubmitProposalAsync(projectId, userId);
                if (!canSubmit)
                {
                    TempData["ErrorMessage"] = "Bạn không thể gửi báo giá cho dự án này.";
                    return RedirectToAction("ProjectDetails", new { id = projectId });
                }

                // Save files
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "proposals");
                Directory.CreateDirectory(uploadsFolder);

                var technicalSolutionFileName = $"{projectId}_{userId}_technical_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(technicalSolution.FileName)}";
                var technicalSolutionPath = Path.Combine(uploadsFolder, technicalSolutionFileName);
                using (var stream = new FileStream(technicalSolutionPath, FileMode.Create))
                {
                    await technicalSolution.CopyToAsync(stream);
                }

                var capabilityProfileFileName = $"{projectId}_{userId}_capability_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(capabilityProfile.FileName)}";
                var capabilityProfilePath = Path.Combine(uploadsFolder, capabilityProfileFileName);
                using (var stream = new FileStream(capabilityProfilePath, FileMode.Create))
                {
                    await capabilityProfile.CopyToAsync(stream);
                }

                // Submit proposal via service
                var dto = new ProposalSubmissionDto
                {
                    GiaiPhapKyThuat = $"/uploads/proposals/{technicalSolutionFileName}",
                    BaoGiaSoBo = preliminaryPrice,
                    ThoiGianTrienKhai = implementationTime!,
                    HoSoNangLucDinhKem = $"/uploads/proposals/{capabilityProfileFileName}"
                };

                await _proposalService.SubmitProposalAsync(projectId, userId, dto);

                // Log submission
                await LogAccessAsync(projectId, "SubmitProposal", $"Price: {preliminaryPrice}, Time: {implementationTime}");

                TempData["SuccessMessage"] = "Gửi báo giá thành công!";
                return RedirectToAction("ProjectDetails", new { id = projectId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("ProjectDetails", new { id = projectId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("ProjectDetails", new { id = projectId });
            }
        }

        // GET: /Seller/ViewMyProposal?projectId=7
        public async Task<IActionResult> ViewMyProposal(int projectId)
        {
            var userId = GetCurrentUserId();

            // Guard: Check if user has invitation
            var invitation = await _context.RFQInvitations
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId &&
                                         i.SellerId == userId &&
                                         i.IsActive);

            if (invitation == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa được mời tham gia dự án này.";
                return RedirectToAction("InvitedProjects");
            }

            // Get seller's proposal
            var proposal = await _context.ProposalSubmissions
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.NguoiTao == userId);

            if (proposal == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa gửi báo giá cho dự án này.";
                return RedirectToAction("ProjectDetails", new { id = projectId });
            }

            // Prepare view model
            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = invitation.Project?.ProjectName;
            ViewBag.ProposalStatus = GetProposalStatusName(proposal.StatusId);
            ViewBag.SubmittedDate = proposal.SubmittedDate;

            return View(proposal);
        }

        // Helper: Get proposal status name
        private string GetProposalStatusName(int statusId)
        {
            return statusId switch
            {
                0 => "Nháp",
                1 => "Đã gửi",
                2 => "Được chọn",
                3 => "Bị từ chối",
                _ => "Không xác định"
            };
        }
    }
}
