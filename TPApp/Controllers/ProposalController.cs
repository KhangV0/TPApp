using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class ProposalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProposalController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: /Proposal/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Proposal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProposalSubmission model, IFormFile? GiaiPhapFile, IFormFile? HoSoFile)
        {
            if (GiaiPhapFile == null || GiaiPhapFile.Length == 0)
            {
                ModelState.AddModelError("GiaiPhapKyThuat", "Vui lòng tải lên giải pháp kỹ thuật.");
            }

            if (HoSoFile == null || HoSoFile.Length == 0)
            {
                ModelState.AddModelError("HoSoNangLucDinhKem", "Vui lòng tải lên hồ sơ năng lực.");
            }

            // Remove ModelState errors for file paths as they are set manually
            ModelState.Remove("GiaiPhapKyThuat");
            ModelState.Remove("HoSoNangLucDinhKem");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle File Uploads
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "proposals");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Save Solutions File
                    if (GiaiPhapFile != null)
                    {
                        string giaiPhapFileName = $"{Guid.NewGuid()}_{GiaiPhapFile.FileName}";
                        string giaiPhapPath = Path.Combine(uploadFolder, giaiPhapFileName);
                        using (var stream = new FileStream(giaiPhapPath, FileMode.Create))
                        {
                            await GiaiPhapFile.CopyToAsync(stream);
                        }
                        model.GiaiPhapKyThuat = $"/uploads/proposals/{giaiPhapFileName}";
                    }

                    // Save Profile File
                    if (HoSoFile != null)
                    {
                        string hoSoFileName = $"{Guid.NewGuid()}_{HoSoFile.FileName}";
                        string hoSoPath = Path.Combine(uploadFolder, hoSoFileName);
                        using (var stream = new FileStream(hoSoPath, FileMode.Create))
                        {
                            await HoSoFile.CopyToAsync(stream);
                        }
                        model.HoSoNangLucDinhKem = $"/uploads/proposals/{hoSoFileName}";
                    }

                    // Set Metadata
                    model.NguoiTao = _userManager.GetUserId(User);
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.ProposalSubmissions.Add(model);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Success));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tải lên hồ sơ: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Proposal/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
