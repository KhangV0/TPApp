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

        // GET: /Project/Workflow/5
        [HttpGet]
        public async Task<IActionResult> Workflow(int id)
        {
            var userId = _userManager.GetUserId(User);
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
            var userId = _userManager.GetUserId(User);
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

            var userId = _userManager.GetUserId(User);
            var member = await _context.ProjectMembers
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId);

            if (member == null) return Forbid();

            // Check workflow status
            var statuses = new Dictionary<string, int>();

            // 1. TechTransfer
            var tech = await _context.TechTransferRequests.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["TechTransfer"] = tech?.StatusId ?? 0;

            // 2. NDA
            var nda = await _context.NDAAgreements.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["NDA"] = nda?.StatusId ?? 0;

            // 3. RFQ
            var rfq = await _context.RFQRequests.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["RFQ"] = rfq?.StatusId ?? 0;

            // 4. Proposal
            var proposal = await _context.ProposalSubmissions.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["Proposal"] = proposal?.StatusId ?? 0;

            // 5. Negotiation
            var negotiation = await _context.NegotiationForms.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["Negotiation"] = negotiation?.StatusId ?? 0;

            // 6. EContract
            var contract = await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["EContract"] = contract?.StatusId ?? 0;

            // 7. AdvancePayment
            var payment = await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["AdvancePayment"] = payment?.StatusId ?? 0;

            // 8. Implementation
            var log = await _context.ImplementationLogs.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["ImplementationLog"] = log?.StatusId ?? 0;

            // 9. Handover
            var handover = await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["Handover"] = handover?.StatusId ?? 0;

            // 10. Acceptance
            var acceptance = await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["Acceptance"] = acceptance?.StatusId ?? 0;

            // 11. Liquidation
            var liquidation = await _context.LiquidationReports.FirstOrDefaultAsync(x => x.ProjectId == id);
            statuses["Liquidation"] = liquidation?.StatusId ?? 0;

            ViewBag.Statuses = statuses;
            ViewBag.UserRole = member.Role;

            return View(member.Project);
        }
    }
}
