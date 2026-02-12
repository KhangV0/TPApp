using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class NegotiationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;

        public NegotiationController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, Services.IWorkflowService workflowService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _workflowService = workflowService;
        }

        // GET: /Negotiation/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
             if (projectId == null) return NotFound("Project Id is required");

            var userId = _userManager.GetUserId(User);
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 5)
            if (!await _workflowService.CanAccessStep(projectId.Value, 5)) return Forbid();

            var existing = await _context.NegotiationForms.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new NegotiationForm { ProjectId = projectId });
        }

        // POST: /Negotiation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NegotiationForm model, IFormFile? BienBanFile)
        {
            var userId = _userManager.GetUserId(User);
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Validate File if "Upload file" is selected
            if (model.HinhThucKy == "Upload file")
            {
                if (BienBanFile == null || BienBanFile.Length == 0)
                {
                    ModelState.AddModelError("BienBanThuongLuongFile", "Vui lòng tải lên biên bản thương lượng.");
                }
            }
            
            // Remove ModelState error because we manually handle the file path
            ModelState.Remove("BienBanThuongLuongFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle File Upload
                    if (BienBanFile != null && BienBanFile.Length > 0)
                    {
                         // Validate extension
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                        var extension = Path.GetExtension(BienBanFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("BienBanThuongLuongFile", "Chỉ chấp nhận file .pdf, .doc, .docx");
                            return View(model);
                        }

                        // Validate Size (20MB)
                        if (BienBanFile.Length > 20 * 1024 * 1024)
                        {
                            ModelState.AddModelError("BienBanThuongLuongFile", "File không được quá 20MB.");
                            return View(model);
                        }

                        string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "negotiations");
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        string uniqueFileName = $"{Guid.NewGuid()}_{BienBanFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await BienBanFile.CopyToAsync(stream);
                        }
                        model.BienBanThuongLuongFile = $"/uploads/negotiations/{uniqueFileName}";
                    }

                    // Auto-sign logic
                    if (model.HinhThucKy == "E-Sign" || model.HinhThucKy == "OTP")
                    {
                        model.DaKySo = true;
                    }

                    // Set Metadata
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.NegotiationForms.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 5
                    await _workflowService.CompleteStep(model.ProjectId.Value, 5);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Negotiation/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
