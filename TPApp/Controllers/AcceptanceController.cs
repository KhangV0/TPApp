using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Added for AnyAsync and FirstOrDefaultAsync
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class AcceptanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;

        public AcceptanceController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService)
        {
            _context = context;
            _userManager = userManager;
            _workflowService = workflowService;
        }

        // GET: /Acceptance/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound("Project Id is required");

            var userId = _userManager.GetUserId(User);
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 10)
            if (!await _workflowService.CanAccessStep(projectId.Value, 10)) return Forbid();

            var existing = await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            var model = new AcceptanceReport
            {
                ProjectId = projectId,
                NgayNghiemThu = DateTime.Now
            };
            return View(model);
        }

        // POST: /Acceptance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcceptanceReport model)
        {
            var userId = _userManager.GetUserId(User);
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    model.TrangThaiKy = "Chưa ký";
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.AcceptanceReports.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 10
                    await _workflowService.CompleteStep(model.ProjectId.Value, 10);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Acceptance/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: SignAsBenA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenA(int id)
        {
            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null) return NotFound();

            if (string.IsNullOrEmpty(report.ChuKyBenA))
            {
                report.ChuKyBenA = User.Identity?.Name ?? "Admin";
                
                if (!string.IsNullOrEmpty(report.ChuKyBenB))
                {
                    report.TrangThaiKy = "Hoàn tất";
                    report.StatusId = 2;
                }
                else
                {
                    report.TrangThaiKy = "Đã ký 1 bên";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: SignAsBenB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenB(int id)
        {
            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null) return NotFound();

            if (string.IsNullOrEmpty(report.ChuKyBenB))
            {
                report.ChuKyBenB = User.Identity?.Name ?? "Khách hàng";

                if (!string.IsNullOrEmpty(report.ChuKyBenA))
                {
                    report.TrangThaiKy = "Hoàn tất";
                    report.StatusId = 2;
                }
                else
                {
                    report.TrangThaiKy = "Đã ký 1 bên";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
