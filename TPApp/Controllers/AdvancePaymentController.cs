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
        private readonly Services.INotificationQueueService _notifQueue;

        public AdvancePaymentController(AppDbContext context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment, Services.IWorkflowService workflowService,
            Services.INotificationQueueService notifQueue)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
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

        // POST: /AdvancePayment/SaveInline  (AJAX from Step 8)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(
            int projectId, decimal SoTienTamUng, DateTime NgayChuyen,
            string? DaXacNhanNhanTien, IFormFile? ChungTuFile)
        {
            try
            {
                var userId = GetCurrentUserId();

                var entity = await _context.AdvancePaymentConfirmations
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);

                bool isNew = entity == null;
                if (isNew)
                {
                    entity = new AdvancePaymentConfirmation
                    {
                        ProjectId = projectId,
                        NguoiTao = userId,
                        NgayTao = DateTime.Now
                    };
                }

                entity.SoTienTamUng = SoTienTamUng;
                entity.NgayChuyen = NgayChuyen;
                entity.DaXacNhanNhanTien = DaXacNhanNhanTien == "true";
                entity.StatusId = entity.DaXacNhanNhanTien ? 2 : 1;
                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;

                // Handle file upload
                if (ChungTuFile != null && ChungTuFile.Length > 0)
                {
                    var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(ChungTuFile.FileName).ToLowerInvariant();
                    if (!allowedExt.Contains(ext))
                        return Json(new { success = false, message = "Chỉ chấp nhận file .pdf, .jpg, .png" });

                    if (ChungTuFile.Length > 10 * 1024 * 1024)
                        return Json(new { success = false, message = "File không được quá 10MB." });

                    var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "advance-payments");
                    Directory.CreateDirectory(uploadFolder);

                    var uniqueName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadFolder, uniqueName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ChungTuFile.CopyToAsync(stream);
                    }
                    entity.ChungTuChuyenTienFile = $"/uploads/advance-payments/{uniqueName}";
                }

                if (isNew)
                    _context.AdvancePaymentConfirmations.Add(entity);

                await _context.SaveChangesAsync();

                // Complete Step 8
                if (entity.ProjectId.HasValue)
                    await _workflowService.CompleteStep(entity.ProjectId.Value, 8);

                // Notify project members
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    var msg = $"Xác nhận tạm ứng {SoTienTamUng:N0} VNĐ đã được {(isNew ? "tạo" : "cập nhật")}";
                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId, "💰 Bước 8: Tạm ứng", msg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId, "💰 Bước 8: Tạm ứng", msg);
                }

                return Json(new { success = true, message = isNew ? "✅ Đã lưu xác nhận tạm ứng." : "✅ Đã cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
