using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class AdvancePaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;

        public AdvancePaymentController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, Services.IWorkflowService workflowService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
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

        // GET: /AdvancePayment/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
             if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 7)
            if (!await _workflowService.CanAccessStep(projectId.Value, 7)) return Forbid();

            var existing = await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new AdvancePaymentConfirmation { ProjectId = projectId });
        }

        // POST: /AdvancePayment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdvancePaymentConfirmation model, IFormFile? ChungTuFile)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Remove ModelState error because we manually handle the file path
            ModelState.Remove("ChungTuChuyenTienFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle File Upload
                    if (ChungTuFile != null && ChungTuFile.Length > 0)
                    {
                         // Validate extension
                        var allowedExtensions = new[] { ".pdf", ".jpg", ".png" };
                        var extension = Path.GetExtension(ChungTuFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ChungTuChuyenTienFile", "Chỉ chấp nhận file .pdf, .jpg, .png");
                            return View(model);
                        }

                        // Validate Size (20MB)
                        if (ChungTuFile.Length > 20 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ChungTuChuyenTienFile", "File không được quá 20MB.");
                            return View(model);
                        }

                        string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "advance-payments");
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        string uniqueFileName = $"{Guid.NewGuid()}_{ChungTuFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ChungTuFile.CopyToAsync(stream);
                        }
                        model.ChungTuChuyenTienFile = $"/uploads/advance-payments/{uniqueFileName}";
                    }

                    // Set Metadata
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    
                    if (model.DaXacNhanNhanTien)
                    {
                        model.StatusId = 2; // Confirmed
                    }
                    else
                    {
                        model.StatusId = 1; // Pending
                    }

                    _context.AdvancePaymentConfirmations.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 7
                    await _workflowService.CompleteStep(model.ProjectId.Value, 7);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /AdvancePayment/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
