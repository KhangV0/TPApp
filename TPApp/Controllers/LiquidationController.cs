using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class LiquidationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public LiquidationController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: /Liquidation/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Liquidation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LiquidationReport model, IFormFile? HoaDonUpload)
        {
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

                    // Metadata
                    model.NguoiTao = _userManager.GetUserId(User);
                    model.NgayTao = DateTime.Now;

                    _context.LiquidationReports.Add(model);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { id = model.Id });
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
    }
}
