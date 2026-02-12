using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class NDAController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;

        public NDAController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService)
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

        // GET: /NDA/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 2)
            if (!await _workflowService.CanAccessStep(projectId.Value, 2)) return Forbid();

            var existing = await _context.NDAAgreements.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new NDAAgreement { ProjectId = projectId });
        }

        // POST: /NDA/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NDAAgreement model)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (!model.DaDongY)
            {
                ModelState.AddModelError("DaDongY", "Bạn phải đồng ý điều khoản trước khi tiếp tục.");
            }

            if (ModelState.IsValid)
            {
                // Set metadata
                model.NguoiTao = userId;
                model.NgayTao = DateTime.Now;
                model.StatusId = 1;

                _context.NDAAgreements.Add(model);
                await _context.SaveChangesAsync();

                // Complete Step 2
                await _workflowService.CompleteStep(model.ProjectId.Value, 2);

                return RedirectToAction("Details", "Project", new { id = model.ProjectId });
            }

            return View(model);
        }

        // GET: /NDA/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
