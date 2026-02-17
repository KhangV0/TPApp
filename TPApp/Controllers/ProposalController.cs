using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For ViewBag if needed
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Authorization;

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

        // GET: /Proposal/Index?duAnId=5
        [HttpGet]
        public async Task<IActionResult> Index(int? duAnId)
        {
            if (duAnId == null) return NotFound("ProjectId is required");

            var userId = GetCurrentUserId();
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
        [RequireInvitedSeller]
        [RequireProjectNdaSigned]
        public async Task<IActionResult> Create(int? duAnId)
        {
            if (duAnId == null) return NotFound("ProjectId is required");

            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == duAnId && m.UserId == userId);

            if (member == null) return Forbid();
            if (member.Role == 1) return Forbid(); // Buyer cannot create

            // SECURITY CHECK: Verify deadline
            var rfq = await _context.RFQRequests
                .FirstOrDefaultAsync(r => r.ProjectId == duAnId);
            if (rfq != null && rfq.HanChotNopHoSo < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Hạn chót nộp hồ sơ đã qua. Bạn không thể gửi báo giá cho dự án này.";
                return RedirectToAction("InvitedProjects", "Seller");
            }

            // Check if already submitted
            var existingProposal = await _context.ProposalSubmissions
                .FirstOrDefaultAsync(p => p.ProjectId == duAnId && p.NguoiTao == userId);
            if (existingProposal != null)
            {
                TempData["InfoMessage"] = "Bạn đã gửi báo giá cho dự án này rồi.";
                return RedirectToAction("Index", new { duAnId = duAnId });
            }

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
            var userId = GetCurrentUserId();
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
                    // SECURITY CHECK: Verify deadline again before saving
                    var rfq = await _context.RFQRequests
                        .FirstOrDefaultAsync(r => r.ProjectId == model.ProjectId);
                    if (rfq != null && rfq.HanChotNopHoSo < DateTime.Now)
                    {
                        TempData["ErrorMessage"] = "Hạn chót nộp hồ sơ đã qua.";
                        return RedirectToAction("InvitedProjects", "Seller");
                    }

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
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1; // Draft

                    _context.ProposalSubmissions.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 4
                    await _workflowService.CompleteStep(model.ProjectId.Value, 4);

                    // Log proposal submission
                    await LogProposalActionAsync(model.ProjectId.Value, userId, "SubmitProposal", 
                        $"ProposalId: {model.Id}");

                    return RedirectToAction("Index", new { duAnId = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // Helper: Log proposal actions for audit trail
        private async Task LogProposalActionAsync(int projectId, int userId, string action, string? additionalData = null)
        {
            var log = new TPApp.Entities.ProjectAccessLog
            {
                ProjectId = projectId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                AdditionalData = additionalData
            };
            _context.ProjectAccessLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // GET: /Proposal/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proposal = await _context.ProposalSubmissions.FindAsync(id);
            if (proposal == null) return NotFound();

            var userId = GetCurrentUserId();
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

            var userId = GetCurrentUserId();
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
                    proposal.NguoiSua = userId;
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

            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == proposal.ProjectId && m.UserId == userId);

            if (member == null) return Forbid();
            
            return View(proposal);
        }
    }
}
