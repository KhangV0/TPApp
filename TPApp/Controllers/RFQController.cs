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

        public RFQController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService)
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
        public async Task<IActionResult> SendToSuppliers(int id)
        {
            var rfq = await _context.RFQRequests.FindAsync(id);
            if (rfq == null)
            {
                return NotFound();
            }

            rfq.DaGuiNhaCungUng = true;
            rfq.StatusId = 2; // Sent status or similar
            _context.Update(rfq);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
