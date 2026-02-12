using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Services;

namespace TPApp.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;

        public ProjectController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService)
        {
            _context = context;
            _userManager = userManager;
            _workflowService = workflowService;
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

        // GET: /Project/Workflow/5
        [HttpGet]
        public async Task<IActionResult> Workflow(int id)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == id && m.UserId == userId);
            if (!isMember) return Forbid();

            var steps = await _workflowService.GetProjectSteps(id);
            ViewBag.ProjectId = id;
            
            var project = await _context.Projects.FindAsync(id);
            ViewBag.ProjectName = project?.ProjectName;

            return View(steps);
        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var projects = await _context.ProjectMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.Project)
                .Select(m => m.Project)
                .ToListAsync();

            return View(projects);
        }

        // GET: /Project/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId);

            if (member == null) return Forbid();

            // Get step statuses
            var statuses = await GetProjectStepStatuses(id.Value);

            // Build step navigation
            var steps = BuildStepNavigation(statuses);

            // Calculate current step (first incomplete step)
            var currentStep = 1;
            for (int i = 0; i < 11; i++)
            {
                if (steps[i].StatusId == 0)
                {
                    currentStep = i + 1;
                    break;
                }
                if (i == 10) currentStep = 11; // All completed
            }

            // Mark current step
            steps[currentStep - 1].IsCurrent = true;

            // Calculate progress
            var completedCount = statuses.Values.Count(s => s > 0);
            var progressPercent = (int)Math.Round((completedCount / 11.0) * 100);

            var viewModel = new TPApp.ViewModel.ProjectDetailWithStepsVm
            {
                Project = member.Project,
                Steps = steps,
                CurrentStep = currentStep,
                UserRole = member.Role,
                ProgressPercent = progressPercent
            };

            return View(viewModel);
        }

        // Helper: Get step statuses
        private async Task<Dictionary<string, int>> GetProjectStepStatuses(int projectId)
        {
            var statuses = new Dictionary<string, int>();

            var tech = await _context.TechTransferRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["TechTransfer"] = tech?.StatusId ?? 0;

            var nda = await _context.NDAAgreements.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["NDA"] = nda?.StatusId ?? 0;

            var rfq = await _context.RFQRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["RFQ"] = rfq?.StatusId ?? 0;

            var proposal = await _context.ProposalSubmissions.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Proposal"] = proposal?.StatusId ?? 0;

            var negotiation = await _context.NegotiationForms.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Negotiation"] = negotiation?.StatusId ?? 0;

            var contract = await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["EContract"] = contract?.StatusId ?? 0;

            var payment = await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["AdvancePayment"] = payment?.StatusId ?? 0;

            var log = await _context.ImplementationLogs.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["ImplementationLog"] = log?.StatusId ?? 0;

            var handover = await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Handover"] = handover?.StatusId ?? 0;

            var acceptance = await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Acceptance"] = acceptance?.StatusId ?? 0;

            var liquidation = await _context.LiquidationReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Liquidation"] = liquidation?.StatusId ?? 0;

            return statuses;
        }

        // Helper: Build step navigation list
        private List<TPApp.ViewModel.ProjectStepNavVm> BuildStepNavigation(Dictionary<string, int> statuses)
        {
            var steps = new List<TPApp.ViewModel.ProjectStepNavVm>
            {
                new() { StepNumber = 1, StepName = "Yêu cầu chuyển giao", StatusId = statuses["TechTransfer"], ControllerName = "TechTransfer", ActionName = "Details", IsAccessible = true },
                new() { StepNumber = 2, StepName = "Thỏa thuận NDA", StatusId = statuses["NDA"], ControllerName = "NDA", ActionName = "Create", IsAccessible = statuses["TechTransfer"] > 0 },
                new() { StepNumber = 3, StepName = "Yêu cầu báo giá", StatusId = statuses["RFQ"], ControllerName = "RFQ", ActionName = "Create", IsAccessible = statuses["NDA"] > 0 },
                new() { StepNumber = 4, StepName = "Nộp hồ sơ", StatusId = statuses["Proposal"], ControllerName = "Proposal", ActionName = "Index", IsAccessible = statuses["RFQ"] > 0 },
                new() { StepNumber = 5, StepName = "Đàm phán", StatusId = statuses["Negotiation"], ControllerName = "Negotiation", ActionName = "Create", IsAccessible = statuses["Proposal"] > 0 },
                new() { StepNumber = 6, StepName = "Ký hợp đồng", StatusId = statuses["EContract"], ControllerName = "EContract", ActionName = "Create", IsAccessible = statuses["Negotiation"] > 0 },
                new() { StepNumber = 7, StepName = "Xác nhận tạm ứng", StatusId = statuses["AdvancePayment"], ControllerName = "AdvancePayment", ActionName = "Create", IsAccessible = statuses["EContract"] > 0 },
                new() { StepNumber = 8, StepName = "Nhật ký triển khai", StatusId = statuses["ImplementationLog"], ControllerName = "ImplementationLog", ActionName = "Create", IsAccessible = statuses["AdvancePayment"] > 0 },
                new() { StepNumber = 9, StepName = "Bàn giao", StatusId = statuses["Handover"], ControllerName = "Handover", ActionName = "Create", IsAccessible = statuses["ImplementationLog"] > 0 },
                new() { StepNumber = 10, StepName = "Nghiệm thu", StatusId = statuses["Acceptance"], ControllerName = "Acceptance", ActionName = "Create", IsAccessible = statuses["Handover"] > 0 },
                new() { StepNumber = 11, StepName = "Thanh lý", StatusId = statuses["Liquidation"], ControllerName = "Liquidation", ActionName = "Create", IsAccessible = statuses["Acceptance"] > 0 }
            };

            return steps;
        }

        // GET: /Project/Step - AJAX endpoint for loading step content
        [HttpGet]
        public async Task<IActionResult> Step(int projectId, int stepNumber)
        {
            // Validate project exists and user has access
            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
            
            if (member == null) return Forbid();
            
            // Validate step number
            if (stepNumber < 1 || stepNumber > 11) return BadRequest("Invalid step number");
            
            // Get step statuses
            var statuses = await GetProjectStepStatuses(projectId);
            var steps = BuildStepNavigation(statuses);
            
            // Determine current step (first incomplete)
            var currentStep = 1;
            for (int i = 0; i < 11; i++)
            {
                if (steps[i].StatusId == 0)
                {
                    currentStep = i + 1;
                    break;
                }
                if (i == 10) currentStep = 11; // All complete
            }
            
            // Validate access - can't skip ahead
            if (stepNumber > currentStep)
            {
                // Return current step instead
                stepNumber = currentStep;
            }
            
            // Build model for partial view
            var model = new TPApp.ViewModel.StepContentVm
            {
                ProjectId = projectId,
                StepNumber = stepNumber,
                StepName = steps[stepNumber - 1].StepName,
                Project = member.Project,
                UserRole = member.Role,
                Steps = steps,
                CurrentStep = currentStep
            };
            
            // Return appropriate partial view
            return PartialView($"Steps/_Step{stepNumber}", model);
        }
    }
}
