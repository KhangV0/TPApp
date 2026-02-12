using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class EContractController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;

        public EContractController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, Services.IWorkflowService workflowService)
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

        // GET: /EContract/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
             if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 6)
            if (!await _workflowService.CanAccessStep(projectId.Value, 6)) return Forbid();

            var existing = await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new EContract { ProjectId = projectId });
        }

        // POST: /EContract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EContract model, IFormFile? ContractFile)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (ContractFile == null || ContractFile.Length == 0)
            {
                ModelState.AddModelError("FileHopDong", "Vui lòng tải lên file hợp đồng.");
            }

            // Remove ModelState error because we manually handle the file path
            ModelState.Remove("FileHopDong");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle File Upload
                    if (ContractFile != null && ContractFile.Length > 0)
                    {
                         // Validate extension
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                        var extension = Path.GetExtension(ContractFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("FileHopDong", "Chỉ chấp nhận file .pdf, .doc, .docx");
                            return View(model);
                        }

                        // Validate Size (20MB)
                        if (ContractFile.Length > 20 * 1024 * 1024)
                        {
                            ModelState.AddModelError("FileHopDong", "File không được quá 20MB.");
                            return View(model);
                        }

                        string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "contracts");
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        string uniqueFileName = $"{Guid.NewGuid()}_{ContractFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ContractFile.CopyToAsync(stream);
                        }
                        model.FileHopDong = $"/uploads/contracts/{uniqueFileName}";
                    }

                    // Set Metadata
                    model.TrangThaiKy = "Chưa ký";
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.EContracts.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 6
                    await _workflowService.CompleteStep(model.ProjectId.Value, 6);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /EContract/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eContract = await _context.EContracts.FindAsync(id);
            if (eContract == null)
            {
                return NotFound();
            }

            return View(eContract);
        }

        // POST: /EContract/SignAsBenA/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenA(int id)
        {
            var eContract = await _context.EContracts.FindAsync(id);
            if (eContract == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(eContract.NguoiKyBenA))
            {
                TempData["Message"] = "Bên A đã ký!";
                return RedirectToAction(nameof(Details), new { id = eContract.Id });
            }

            // Simulate signing
            var user = await _userManager.GetUserAsync(User);
            eContract.NguoiKyBenA = $"{user.UserName} - Giám Đốc - {Guid.NewGuid()}";
            eContract.NgaySua = DateTime.Now;
            eContract.NguoiSua = GetCurrentUserId();

            // Update Status
            if (string.IsNullOrEmpty(eContract.NguoiKyBenB))
            {
                eContract.TrangThaiKy = "Đã ký 1 bên";
            }
            else
            {
                eContract.TrangThaiKy = "Đã hoàn tất";
            }

            _context.Update(eContract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = eContract.Id });
        }

        // POST: /EContract/SignAsBenB/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenB(int id)
        {
            var eContract = await _context.EContracts.FindAsync(id);
            if (eContract == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(eContract.NguoiKyBenB))
            {
                TempData["Message"] = "Bên B đã ký!";
                return RedirectToAction(nameof(Details), new { id = eContract.Id });
            }

            // Simulate signing
            var user = await _userManager.GetUserAsync(User);
            eContract.NguoiKyBenB = $"{user.UserName} - Đối tác - {Guid.NewGuid()}";
            eContract.NgaySua = DateTime.Now;
            eContract.NguoiSua = GetCurrentUserId();

            // Update Status
            if (string.IsNullOrEmpty(eContract.NguoiKyBenA))
            {
                eContract.TrangThaiKy = "Đã ký 1 bên";
            }
            else
            {
                eContract.TrangThaiKy = "Đã hoàn tất";
            }

            _context.Update(eContract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = eContract.Id });
        }
    }
}
