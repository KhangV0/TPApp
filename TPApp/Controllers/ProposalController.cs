using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For ViewBag if needed
using Microsoft.EntityFrameworkCore;
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
        private readonly Services.IWorkflowService _workflowService;

        public ProposalController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, Services.IWorkflowService workflowService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _workflowService = workflowService;
        }

        // GET: /Proposal/Index?duAnId=5
        [HttpGet]
        public async Task<IActionResult> Index(int? duAnId)
        {
            if (duAnId == null) return NotFound("ProjectId is required");

            var userId = _userManager.GetUserId(User);
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == duAnId && m.UserId == userId);
            
            if (member == null) return Forbid();

            var proposals = await _context.ProposalSubmissions
                .Where(p => p.ProjectId == duAnId)
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();

            ViewBag.ProjectId = duAnId;
            ViewBag.UserRole = member.Role; // 1=Buyer, 2=Seller, 3=Consultant

            return View(proposals);
        }

        // GET: /Proposal/Create?duAnId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? duAnId)
        {
            if (duAnId == null) return NotFound("ProjectId is required");

            var userId = _userManager.GetUserId(User);
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == duAnId && m.UserId == userId);

            if (member == null) return Forbid();
            if (member.Role == 1) return Forbid(); // Buyer cannot create

            var model = new ProposalSubmission
            {
                ProjectId = duAnId
            };
            return View(model);
        }

        // POST: /Proposal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProposalSubmission model, IFormFile? GiaiPhapFile, IFormFile? HoSoFile)
        {
            var userId = _userManager.GetUserId(User);
            var member = await _context.ProjectMembers
                 .FirstOrDefaultAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);

            if (member == null) return Forbid();
            if (member.Role == 1) return Forbid(); // Buyer cannot create

            if (GiaiPhapFile == null || GiaiPhapFile.Length == 0)
                ModelState.AddModelError("GiaiPhapKyThuat", "Vui lòng tải lên giải pháp kỹ thuật.");
            
            if (HoSoFile == null || HoSoFile.Length == 0)
                ModelState.AddModelError("HoSoNangLucDinhKem", "Vui lòng tải lên hồ sơ năng lực.");

            // Remove ModelState errors for file paths as they are set manually
            ModelState.Remove("GiaiPhapKyThuat");
            ModelState.Remove("HoSoNangLucDinhKem");
            ModelState.Remove("NguoiTao"); // Auto-set
            ModelState.Remove("NgayTao"); // Auto-set

            if (ModelState.IsValid)
            {
                try
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "proposals");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    // Save Solutions File
                    if (GiaiPhapFile != null)
                    {
                        string fileName = $"{Guid.NewGuid()}_{GiaiPhapFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) await GiaiPhapFile.CopyToAsync(stream);
                        model.GiaiPhapKyThuat = $"/uploads/proposals/{fileName}";
                    }

                    // Save Profile File
                    if (HoSoFile != null)
                    {
                        string fileName = $"{Guid.NewGuid()}_{HoSoFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) await HoSoFile.CopyToAsync(stream);
                        model.HoSoNangLucDinhKem = $"/uploads/proposals/{fileName}";
                    }

                    // Set Metadata
                    model.NguoiTao = User.Identity?.Name ?? userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1; // Draft

                    _context.ProposalSubmissions.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 4
                    await _workflowService.CompleteStep(model.ProjectId.Value, 4);

                    return RedirectToAction("Index", new { duAnId = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Proposal/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proposal = await _context.ProposalSubmissions.FindAsync(id);
            if (proposal == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == proposal.ProjectId && m.UserId == userId);

            if (member == null) return Forbid();
            if (member.Role == 1) return Forbid(); // Buyer cannot edit

            return View(proposal);
        }

        // POST: /Proposal/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProposalSubmission model, IFormFile? GiaiPhapFile, IFormFile? HoSoFile)
        {
            if (id != model.Id) return NotFound();

            var proposal = await _context.ProposalSubmissions.FindAsync(id);
            if (proposal == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == proposal.ProjectId && m.UserId == userId);

            if (member == null) return Forbid();
            if (member.Role == 1) return Forbid(); // Buyer cannot edit

            ModelState.Remove("GiaiPhapKyThuat"); // Optional if not changing
            ModelState.Remove("HoSoNangLucDinhKem"); // Optional if not changing

            if (ModelState.IsValid)
            {
                try
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "proposals");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    if (GiaiPhapFile != null)
                    {
                        string fileName = $"{Guid.NewGuid()}_{GiaiPhapFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) await GiaiPhapFile.CopyToAsync(stream);
                        proposal.GiaiPhapKyThuat = $"/uploads/proposals/{fileName}";
                    }

                    if (HoSoFile != null)
                    {
                        string fileName = $"{Guid.NewGuid()}_{HoSoFile.FileName}";
                        string filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) await HoSoFile.CopyToAsync(stream);
                        proposal.HoSoNangLucDinhKem = $"/uploads/proposals/{fileName}";
                    }

                    proposal.BaoGiaSoBo = model.BaoGiaSoBo;
                    proposal.ThoiGianTrienKhai = model.ThoiGianTrienKhai;
                    proposal.NguoiSua = User.Identity?.Name ?? userId;
                    proposal.NgaySua = DateTime.Now;

                    _context.Update(proposal);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", new { duAnId = proposal.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            return View(model);
        }

        // GET: /Proposal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

             var proposal = await _context.ProposalSubmissions.FindAsync(id);
            if (proposal == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == proposal.ProjectId && m.UserId == userId);

            if (member == null) return Forbid();
            
            return View(proposal);
        }
    }
}
