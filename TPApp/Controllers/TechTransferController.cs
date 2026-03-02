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
        private readonly Services.INotificationQueueService _notifQueue;

        public TechTransferController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService, Services.INotificationQueueService notifQueue)
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

        // GET: /TechTransfer/Create?fromId=18&typeData=1
        [HttpGet]
        public async Task<IActionResult> Create(int? fromId = null, int? typeData = null)
        {
            ViewBag.LinhVucList = await _context.Categories.AsNoTracking()
                .Where(c => c.ParentId == 1)
                .OrderBy(c => c.Title)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Title,
                    Text = c.Title
                })
                .ToListAsync();

            // Pre-fill contact info from current user
            var user = await _userManager.GetUserAsync(User);
            var model = new TechTransferRequest();
            if (user != null)
            {
                model.HoTen = user.FullName ?? "";
                model.DienThoai = user.Phone ?? user.PhoneNumber ?? "";
                model.Email = user.Email ?? "";
                model.DiaChi = user.DiaChi ?? "";
            }

            // Store source reference
            model.FromId = fromId;
            model.TypeData = typeData;

            // Pre-fill product info from SanPhamCNTB (typeData=1)
            if (fromId.HasValue && (typeData ?? 1) == 1)
            {
                var product = await _context.SanPhamCNTBs.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ID == fromId.Value);

                if (product != null)
                {
                    model.TenCongNghe = product.Name ?? "";

                    // Lookup category title
                    var catId = await _context.SanPhamCNTBCategories
                        .Where(x => x.SanPhamCNTBId == fromId.Value)
                        .Select(x => x.CatId)
                        .FirstOrDefaultAsync();

                    if (catId > 0)
                    {
                        model.LinhVuc = await _context.Categories
                            .Where(x => x.CatId == catId)
                            .Select(x => x.Title)
                            .FirstOrDefaultAsync() ?? "";
                    }
                }
            }

            return View(model);
        }

        // POST: /TechTransfer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechTransferRequest model)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                   try
                   {
                        var userId = GetCurrentUserId();

                        // Truncate to match DB column limits
                        var projectName = ("Dự án: " + model.TenCongNghe);
                        if (projectName.Length > 500) projectName = projectName[..500];

                        // 1. Create Project
                        var project = new Project
                        {
                            ProjectName = projectName,
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

                         // Notify buyer: project created, proceed to Step 2
                         await _notifQueue.QueueAsync(userId, project.Id,
                             " Dự án đã được tạo",
                             $"Yêu cầu chuyển giao '{model.TenCongNghe}' đã ghi nhận thành công. Tiến hành bước 2: Ký NDA.");

                        await transaction.CommitAsync();

                        return RedirectToAction("Details", "Project", new { id = project.Id });
                   }
                   catch (Exception ex)
                   {
                       await transaction.RollbackAsync();
                       var detail = ex.InnerException?.Message ?? ex.Message;
                       ModelState.AddModelError("", "Lỗi tạo dự án: " + detail);
                   }
                }
            }

            // Reload categories on validation failure
            ViewBag.LinhVucList = await _context.Categories.AsNoTracking()
                .Where(c => c.ParentId == 1)
                .OrderBy(c => c.Title)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Title,
                    Text = c.Title
                })
                .ToListAsync();

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
                new() { StepNumber = 1, StepName = "YÃªu cáº§u chuyá»ƒn giao", StatusId = statuses["TechTransfer"], ControllerName = "TechTransfer", ActionName = "Details", IsAccessible = true },
                new() { StepNumber = 2, StepName = "Thá»a thuáº­n NDA", StatusId = statuses["NDA"], ControllerName = "NDA", ActionName = "Create", IsAccessible = statuses["TechTransfer"] > 0 },
                new() { StepNumber = 3, StepName = "YÃªu cáº§u bÃ¡o giÃ¡", StatusId = statuses["RFQ"], ControllerName = "RFQ", ActionName = "Create", IsAccessible = statuses["NDA"] > 0 },
                new() { StepNumber = 4, StepName = "Ná»™p há»“ sÆ¡", StatusId = statuses["Proposal"], ControllerName = "Proposal", ActionName = "Index", IsAccessible = statuses["RFQ"] > 0 },
                new() { StepNumber = 5, StepName = "ÄÃ m phÃ¡n", StatusId = statuses["Negotiation"], ControllerName = "Negotiation", ActionName = "Create", IsAccessible = statuses["Proposal"] > 0 },
                new() { StepNumber = 6, StepName = "KÃ½ há»£p Ä‘á»“ng", StatusId = statuses["EContract"], ControllerName = "EContract", ActionName = "Create", IsAccessible = statuses["Negotiation"] > 0 },
                new() { StepNumber = 7, StepName = "XÃ¡c nháº­n táº¡m á»©ng", StatusId = statuses["AdvancePayment"], ControllerName = "AdvancePayment", ActionName = "Create", IsAccessible = statuses["EContract"] > 0 },
                new() { StepNumber = 8, StepName = "Nháº­t kÃ½ triá»ƒn khai", StatusId = statuses["ImplementationLog"], ControllerName = "ImplementationLog", ActionName = "Create", IsAccessible = statuses["AdvancePayment"] > 0 },
                new() { StepNumber = 9, StepName = "BÃ n giao", StatusId = statuses["Handover"], ControllerName = "Handover", ActionName = "Create", IsAccessible = statuses["ImplementationLog"] > 0 },
                new() { StepNumber = 10, StepName = "Nghiá»‡m thu", StatusId = statuses["Acceptance"], ControllerName = "Acceptance", ActionName = "Create", IsAccessible = statuses["Handover"] > 0 },
                new() { StepNumber = 11, StepName = "Thanh lÃ½", StatusId = statuses["Liquidation"], ControllerName = "Liquidation", ActionName = "Create", IsAccessible = statuses["Acceptance"] > 0 }
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

