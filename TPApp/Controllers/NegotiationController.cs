using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class NegotiationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public NegotiationController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: /Negotiation/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Negotiation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NegotiationForm model, IFormFile? BienBanFile)
        {
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
                    model.NguoiTao = _userManager.GetUserId(User);
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.NegotiationForms.Add(model);
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

        // GET: /Negotiation/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
