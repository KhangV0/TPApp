using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class ImplementationLogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;

        public ImplementationLogController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, Services.IWorkflowService workflowService)
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

        // GET: /ImplementationLog/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
             if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 8)
            if (!await _workflowService.CanAccessStep(projectId.Value, 8)) return Forbid();

            var existing = await _context.ImplementationLogs.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new ImplementationLog { ProjectId = projectId });
        }

        // POST: /ImplementationLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImplementationLog model, IFormFile? MediaFile, IFormFile? BienBanFile)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Remove ModelState error because we manually handle the file paths
            ModelState.Remove("HinhAnhVideoFile");
            ModelState.Remove("BienBanXacNhanFile");

            if (ModelState.IsValid)
            {
                try
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "implementation-logs");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Handle Media File Upload (Images/Videos)
                    if (MediaFile != null && MediaFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".png", ".mp4", ".mov", ".pdf" };
                        var extension = Path.GetExtension(MediaFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("HinhAnhVideoFile", "Chỉ chấp nhận file .jpg, .png, .mp4, .mov, .pdf");
                            return View(model);
                        }

                        if (MediaFile.Length > 50 * 1024 * 1024)
                        {
                            ModelState.AddModelError("HinhAnhVideoFile", "File hình ảnh/video không được quá 50MB.");
                            return View(model);
                        }

                        string uniqueFileName = $"{Guid.NewGuid()}_{MediaFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await MediaFile.CopyToAsync(stream);
                        }
                        model.HinhAnhVideoFile = $"/uploads/implementation-logs/{uniqueFileName}";
                    }

                    // Handle Minutes File Upload (Docs)
                    if (BienBanFile != null && BienBanFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                        var extension = Path.GetExtension(BienBanFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("BienBanXacNhanFile", "Chỉ chấp nhận file .pdf, .doc, .docx");
                            return View(model);
                        }

                        if (BienBanFile.Length > 20 * 1024 * 1024)
                        {
                            ModelState.AddModelError("BienBanXacNhanFile", "File biên bản không được quá 20MB.");
                            return View(model);
                        }

                        string uniqueFileName = $"{Guid.NewGuid()}_{BienBanFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await BienBanFile.CopyToAsync(stream);
                        }
                        model.BienBanXacNhanFile = $"/uploads/implementation-logs/{uniqueFileName}";
                    }

                    // Set Metadata
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.ImplementationLogs.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 8
                    await _workflowService.CompleteStep(model.ProjectId.Value, 8);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /ImplementationLog/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
