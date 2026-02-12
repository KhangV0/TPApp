using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
// #if false
    [Authorize]
    public class TechTransferController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;

        public TechTransferController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService)
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

        // GET: /TechTransfer/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /TechTransfer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechTransferRequest model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                   try
                   {
                        var userId = GetCurrentUserId();

                        // 1. Create Project
                        var project = new Project
                        {
                            ProjectName = "Dự án: " + model.TenCongNghe,
                            ProjectCode = "PJ-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            Description = model.MoTaNhuCau,
                            StatusId = 1, // Active
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        };
                        _context.Projects.Add(project);
                        await _context.SaveChangesAsync();

                        // 2. Add Member (Buyer)
                        var member = new ProjectMember
                        {
                            ProjectId = project.Id,
                            UserId = userId,
                            Role = 1, // Buyer
                            JoinedDate = DateTime.Now,
                            IsActive = true
                        };
                        _context.ProjectMembers.Add(member);
                        await _context.SaveChangesAsync();

                        // 3. Create TechTransferRequest linked to Project
                        model.ProjectId = project.Id;
                        model.NguoiTao = userId;
                        model.NgayTao = DateTime.Now;
                        model.StatusId = 1;

                        _context.TechTransferRequests.Add(model);
                        await _context.SaveChangesAsync();

                        // 4. Initialize and Complete Step 1
                        await _workflowService.InitializeProjectSteps(project.Id);
                        await _workflowService.CompleteStep(project.Id, 1);

                        transaction.Commit();

                        return RedirectToAction("Details", "Project", new { id = project.Id });
                   }
                   catch (Exception ex)
                   {
                       transaction.Rollback();
                       ModelState.AddModelError("", "Lỗi tạo dự án: " + ex.Message);
                   }
                }
            }
            return View(model);
        }

        // GET: /TechTransfer/Details/{projectId}
        [HttpGet]
        public async Task<IActionResult> Details(int projectId)
        {
            var userId = GetCurrentUserId();

            // Check if user is member of the project
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            var techTransfer = await _context.TechTransferRequests
                .FirstOrDefaultAsync(t => t.ProjectId == projectId);

            if (techTransfer == null) return NotFound();

            // Load step navigation
            await LoadStepNavigation(projectId);

            return View(techTransfer);
        }

        // Helper: Load step navigation for ViewBag
        private async Task LoadStepNavigation(int projectId)
        {
            var statuses = await GetProjectStepStatuses(projectId);
            var steps = BuildStepNavigation(statuses);
            
            // Mark current step (Step 1 for TechTransfer)
            steps[0].IsCurrent = true;
            
            ViewBag.ProjectSteps = steps;
            ViewBag.ProjectId = projectId;
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

        // GET: /TechTransfer/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
// #endif
}
