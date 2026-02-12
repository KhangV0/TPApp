using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class AdvancePaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdvancePaymentController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: /AdvancePayment/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /AdvancePayment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdvancePaymentConfirmation model, IFormFile? ChungTuFile)
        {
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
                    model.NguoiTao = _userManager.GetUserId(User);
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

                    return RedirectToAction(nameof(Success));
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
