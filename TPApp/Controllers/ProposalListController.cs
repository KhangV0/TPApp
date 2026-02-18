using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    [Authorize]
    public class ProposalListController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProposalService _proposalService;
        private readonly IScoringService _scoringService;
        private readonly ISelectionService _selectionService;
        private readonly ILogger<ProposalListController> _logger;

        public ProposalListController(
            AppDbContext context,
            IProposalService proposalService,
            IScoringService scoringService,
            ISelectionService selectionService,
            ILogger<ProposalListController> logger)
        {
            _context = context;
            _proposalService = proposalService;
            _scoringService = scoringService;
            _selectionService = selectionService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: /ProposalList/Index?projectId=7
        public async Task<IActionResult> Index(int projectId)
        {
            var userId = GetCurrentUserId();

            // Guard: User must be project owner or consultant
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (member == null || (member.Role != 1 && member.Role != 3)) // 1=Owner, 3=Consultant
            {
                _logger.LogWarning("User {UserId} attempted to access proposal list for project {ProjectId} without permission", userId, projectId);
                return Forbid();
            }

            // Get project details
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Get all submitted proposals for this project
            var proposals = await _context.ProposalSubmissions
                .Include(p => p.Seller)
                .Where(p => p.ProjectId == projectId && p.StatusId == (int)ProposalStatus.Submitted)
                .ToListAsync();

            // Build view model
            var proposalItems = new List<ProposalItemVm>();

            foreach (var proposal in proposals)
            {
                var scores = await _scoringService.GetProposalScoresAsync(proposal.Id);
                var averageScore = await _scoringService.GetAverageScoreAsync(proposal.Id);

                proposalItems.Add(new ProposalItemVm
                {
                    Proposal = proposal,
                    SellerName = proposal.Seller?.FullName ?? "Unknown",
                    Scores = scores,
                    AverageScore = averageScore,
                    IsSelected = project.SelectedSellerId == proposal.NguoiTao
                });
            }

            var viewModel = new ProposalListVm
            {
                ProjectId = projectId,
                ProjectName = project.ProjectName,
                Proposals = proposalItems,
                IsOwner = member.Role == 1,
                IsConsultant = member.Role == 3,
                SelectedProposalId = proposals.FirstOrDefault(p => p.NguoiTao == project.SelectedSellerId)?.Id
            };

            return View(viewModel);
        }

        // POST: /ProposalList/SelectSeller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectSeller(int projectId, int proposalId)
        {
            var userId = GetCurrentUserId();

            try
            {
                if (!await _selectionService.CanSelectSellerAsync(projectId, proposalId, userId))
                {
                    TempData["ErrorMessage"] = "Bạn không thể chọn nhà cung ứng này. Vui lòng kiểm tra quyền truy cập.";
                    return RedirectToAction("Index", new { projectId });
                }

                await _selectionService.SelectSellerAsync(projectId, proposalId, userId);

                _logger.LogInformation("User {UserId} selected proposal {ProposalId} for project {ProjectId}", userId, proposalId, projectId);

                TempData["SuccessMessage"] = "Đã chọn nhà cung ứng thành công!";
                return RedirectToAction("Step", "Project", new { id = projectId, step = 5 });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error selecting seller for project {ProjectId}", projectId);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { projectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error selecting seller for project {ProjectId}", projectId);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi chọn nhà cung ứng. Vui lòng thử lại.";
                return RedirectToAction("Index", new { projectId });
            }
        }

        // GET: /ProposalList/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            var proposal = await _context.ProposalSubmissions
                .Include(p => p.Seller)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null)
            {
                return NotFound();
            }

            // Guard: User must be project owner or consultant
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == proposal.ProjectId && m.UserId == userId);

            if (member == null || (member.Role != 1 && member.Role != 3))
            {
                return Forbid();
            }

            // Get scores
            var scores = await _scoringService.GetProposalScoresAsync(id);
            var averageScore = await _scoringService.GetAverageScoreAsync(id);
            var consultantScore = await _scoringService.GetConsultantScoreAsync(id, userId);

            ViewBag.Scores = scores;
            ViewBag.AverageScore = averageScore;
            ViewBag.ConsultantScore = consultantScore;
            ViewBag.IsOwner = member.Role == 1;
            ViewBag.IsConsultant = member.Role == 3;
            ViewBag.CanScore = await _scoringService.CanScoreProposalAsync(id, userId);

            return View(proposal);
        }
    }
}
