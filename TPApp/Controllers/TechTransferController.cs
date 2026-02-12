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
                        var userId = _userManager.GetUserId(User);

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

        // GET: /TechTransfer/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
// #endif
}
