using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.EntityFrameworkCore; // Added for FirstOrDefaultAsync

namespace TPApp.Controllers
{
    [Authorize]
    public class LiquidationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public LiquidationController(AppDbContext context, UserManager<ApplicationUser> userManager,
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

        // GET: /Liquidation/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (member == null) return Forbid();
            if (member.Role == 3) return Forbid(); // Consultant/Other restricted?

            // Check Workflow Access (Step 11)
            if (!await _workflowService.CanAccessStep(projectId.Value, 11)) return Forbid();

            var existing = await _context.LiquidationReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            return View(new LiquidationReport { ProjectId = projectId });
        }

        // POST: /Liquidation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LiquidationReport model, IFormFile? HoaDonUpload)
        {
            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (member == null) return Forbid();
            if (member.Role == 3) return Forbid();

            ModelState.Remove("HoaDonFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle File Upload
                    if (HoaDonUpload != null && HoaDonUpload.Length > 0)
                    {
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                        var extension = Path.GetExtension(HoaDonUpload.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("HoaDonFile", "Chỉ chấp nhận file .pdf, .doc, .docx, .xls, .xlsx");
                            return View(model);
                        }

                        if (HoaDonUpload.Length > 20 * 1024 * 1024)
                        {
                            ModelState.AddModelError("HoaDonFile", "File hóa đơn không được quá 20MB.");
                            return View(model);
                        }

                        string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "liquidations");
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        string uniqueFileName = $"{Guid.NewGuid()}_{HoaDonUpload.FileName}";
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await HoaDonUpload.CopyToAsync(stream);
                        }
                        model.HoaDonFile = $"/uploads/liquidations/{uniqueFileName}";
                    }

                    // Logic
                    if (model.SanDaChuyenTien)
                    {
                        model.HopDongClosed = true;
                        model.StatusId = 2;
                    }
                    else
                    {
                        model.StatusId = 1;
                    }

                    // Set Metadata
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.LiquidationReports.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 11
                    await _workflowService.CompleteStep(model.ProjectId.Value, 11);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Liquidation/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.LiquidationReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: /Liquidation/ConfirmPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var report = await _context.LiquidationReports.FindAsync(id);
            if (report == null) return NotFound();

            if (!report.SanDaChuyenTien)
            {
                report.SanDaChuyenTien = true;
                report.HopDongClosed = true;
                report.StatusId = 2;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        // GET: /Liquidation/DownloadInvoice/5
        [HttpGet]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var report = await _context.LiquidationReports.FindAsync(id);
            if (report == null || string.IsNullOrEmpty(report.HoaDonFile))
            {
                return NotFound();
            }

            string cleanPath = report.HoaDonFile.Replace("/", "\\").TrimStart('\\');
            string filePath = Path.Combine(_environment.WebRootPath, cleanPath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server.");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            string contentType = "application/octet-stream";
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".pdf") contentType = "application/pdf";
            else if (extension == ".doc") contentType = "application/msword";
            else if (extension == ".docx") contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if (extension == ".xls") contentType = "application/vnd.ms-excel";
            else if (extension == ".xlsx") contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(memory, contentType, Path.GetFileName(filePath));
        }

        // POST: /Liquidation/SaveInline (AJAX from Step 14)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(
            int projectId, string? GiaTriThanhToanConLai, string? SoHoaDon,
            string? SanDaChuyenTien, string? HopDongClosed,
            IFormFile? HoaDonUpload)
        {
            try
            {
                var userId = GetCurrentUserId();

                var entity = await _context.LiquidationReports
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);

                bool isNew = entity == null;
                if (isNew)
                {
                    entity = new LiquidationReport
                    {
                        ProjectId = projectId,
                        NguoiTao = userId,
                        NgayTao = DateTime.Now
                    };
                }

                // Parse money
                if (!string.IsNullOrWhiteSpace(GiaTriThanhToanConLai))
                {
                    var clean = GiaTriThanhToanConLai.Replace(".", "").Replace(",", "").Trim();
                    if (decimal.TryParse(clean, out decimal val))
                        entity.GiaTriThanhToanConLai = val;
                }

                entity.SoHoaDon = SoHoaDon;
                entity.SanDaChuyenTien = SanDaChuyenTien == "true";
                entity.HopDongClosed = HopDongClosed == "true";
                entity.StatusId = entity.SanDaChuyenTien ? 2 : 1;
                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;

                // Handle invoice file
                if (HoaDonUpload != null && HoaDonUpload.Length > 0)
                {
                    if (HoaDonUpload.Length > 10 * 1024 * 1024)
                        return Json(new { success = false, message = "File hóa đơn không được quá 10MB." });

                    var folder = Path.Combine(_environment.WebRootPath, "uploads", "liquidations");
                    Directory.CreateDirectory(folder);
                    var ext = Path.GetExtension(HoaDonUpload.FileName).ToLowerInvariant();
                    var name = $"{Guid.NewGuid()}{ext}";
                    using (var stream = new FileStream(Path.Combine(folder, name), FileMode.Create))
                        await HoaDonUpload.CopyToAsync(stream);
                    entity.HoaDonFile = $"/uploads/liquidations/{name}";
                }

                if (isNew)
                    _context.LiquidationReports.Add(entity);

                await _context.SaveChangesAsync();

                if (entity.ProjectId.HasValue)
                    await _workflowService.CompleteStep(entity.ProjectId.Value, 14);

                // Notify project members
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    var msg = $"Thanh lý đã được {(isNew ? "tạo" : "cập nhật")}. {(entity.HopDongClosed ? "Hợp đồng đã đóng." : "")}";
                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId, "💵 Bước 14: Thanh lý", msg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId, "💵 Bước 14: Thanh lý", msg);
                }

                return Json(new { success = true, message = isNew ? "✅ Đã lưu thanh lý." : "✅ Đã cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
