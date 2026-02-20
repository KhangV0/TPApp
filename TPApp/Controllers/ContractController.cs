using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private readonly AppDbContext             _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment     _env;
        private readonly IContractService        _contracts;
        private readonly IContractApprovalService _approvals;
        private readonly IContractAuditService   _audit;
        private readonly ILogger<ContractController> _logger;

        public ContractController(
            AppDbContext context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env, IContractService contracts,
            IContractApprovalService approvals, IContractAuditService audit,
            ILogger<ContractController> logger)
        {
            _context   = context;
            _userManager = userManager;
            _env       = env;
            _contracts = contracts;
            _approvals = approvals;
            _audit     = audit;
            _logger    = logger;
        }

        private int GetUserId()
        {
            var s = _userManager.GetUserId(User);
            return int.TryParse(s, out int id) ? id : 0;
        }

        private async Task<(int role, bool canAccess)> GetRoleAsync(int projectId, int userId)
        {
            var proj = await _context.Projects.FindAsync(projectId);
            if (proj == null) return (0, false);
            if (proj.CreatedBy == userId) return (1, true);         // Buyer
            if (proj.SelectedSellerId == userId) return (2, true);  // Seller
            // Use ProjectMembers (consistent with ProjectController)
            var isConsultant = await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId && m.Role == 3);
            if (isConsultant) return (3, true); // Consultant
            return (0, false);
        }

        // ─── GET /Contract/Index?projectId= ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(int projectId)
        {
            var userId = GetUserId();
            var (role, canAccess) = await GetRoleAsync(projectId, userId);
            if (!canAccess) return Forbid();

            var project   = await _context.Projects.FindAsync(projectId);
            var active    = await _contracts.GetActiveContractAsync(projectId);
            var versions  = await _contracts.GetAllVersionsAsync(projectId);
            var approvals = active != null ? await _approvals.GetApprovalSummaryAsync(active.Id) : new();
            var auditLogs = await _context.ContractAuditLogs
                .Where(l => l.EntityName == "ProjectContract" || l.EntityName == "ContractApproval")
                .OrderByDescending(l => l.CreatedDate)
                .Take(30)
                .ToListAsync();

            ViewBag.Project   = project;
            ViewBag.ProjectId = projectId;
            ViewBag.UserRole  = role;
            ViewBag.Versions  = versions;
            ViewBag.Approvals = approvals;
            ViewBag.AuditLogs = auditLogs;
            ViewBag.AllApproved = active != null && await _approvals.AllPartiesApprovedAsync(active.Id);

            return View(active);
        }

        // ─── POST /Contract/CreateAuto  (AJAX) ───────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateAuto([FromBody] ProjectIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                var (role, canAccess) = await GetRoleAsync(dto.ProjectId, userId);
                if (!canAccess || role > 2) return Json(new { success = false, message = "Không có quyền." });

                var contract = await _contracts.AutoCreateDraftAsync(dto.ProjectId, userId);
                return Json(new { success = true, message = "✅ Bản nháp hợp đồng được tạo tự động.", contractId = contract.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAuto error for project {Id}", dto.ProjectId);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ─── POST /Contract/Upload  (form file) ──────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int projectId, IFormFile? contractFile)
        {
            try
            {
                var userId = GetUserId();
                var (role, canAccess) = await GetRoleAsync(projectId, userId);
                if (!canAccess || role > 2) return Forbid();

                if (contractFile == null) return RedirectToAction("Index", new { projectId });

                await _contracts.UploadDraftAsync(projectId, userId, contractFile, _env);
                TempData["Success"] = "📎 Hợp đồng đã được tải lên thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index", new { projectId });
        }

        // ─── POST /Contract/Approve  (AJAX) ───────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Approve([FromBody] ApprovalDecisionDto dto)
        {
            try
            {
                var userId = GetUserId();
                var (role, canAccess) = await GetRoleAsync(dto.ProjectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền." });

                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua = Request.Headers["User-Agent"].ToString();
                var (ok, msg) = await _approvals.SubmitDecisionAsync(
                    dto.ContractId, userId, role, true, dto.Comment, ip, ua);

                return Json(new { success = ok, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── POST /Contract/Reject  (AJAX) ────────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Reject([FromBody] ApprovalDecisionDto dto)
        {
            try
            {
                var userId = GetUserId();
                var (role, canAccess) = await GetRoleAsync(dto.ProjectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền." });

                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua = Request.Headers["User-Agent"].ToString();
                var (ok, msg) = await _approvals.SubmitDecisionAsync(
                    dto.ContractId, userId, role, false, dto.Comment, ip, ua);

                return Json(new { success = ok, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── POST /Contract/SetReadyToSign  (AJAX) ────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SetReadyToSign([FromBody] ContractIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                // Only Buyer can finalize
                var contract = await _context.ProjectContracts.FindAsync(dto.ContractId);
                if (contract == null) return Json(new { success = false, message = "Không tìm thấy." });

                var (role, canAccess) = await GetRoleAsync(contract.ProjectId, userId);
                if (!canAccess || role != 1) return Json(new { success = false, message = "Chỉ Buyer có quyền chốt." });

                var (ok, msg) = await _contracts.SetReadyToSignAsync(dto.ContractId, userId);
                return Json(new { success = ok, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── GET /Contract/Download/{contractId}/{type} ───────────────────────
        [HttpGet]
        public async Task<IActionResult> Download(int contractId, string type = "original")
        {
            var userId = GetUserId();
            (string? Path, string? Name) file;

            if (type == "signed")
                file = await _contracts.GetDownloadSignedAsync(contractId, userId);
            else
                file = await _contracts.GetDownloadOriginalAsync(contractId, userId);

            if (string.IsNullOrEmpty(file.Path) || !System.IO.File.Exists(file.Path))
                return NotFound("File không tồn tại.");

            var bytes = await System.IO.File.ReadAllBytesAsync(file.Path);
            var mime  = file.Name?.EndsWith(".pdf") == true ? "application/pdf"
                      : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            return File(bytes, mime, file.Name ?? "contract.pdf");
        }
    }

    // ─── DTOs ─────────────────────────────────────────────────────────────────
    public class ProjectIdDto       { public int ProjectId  { get; set; } }
    public class ContractIdDto      { public int ContractId { get; set; } }
    public class ApprovalDecisionDto
    {
        public int     ProjectId  { get; set; }
        public int     ContractId { get; set; }
        public string? Comment    { get; set; }
    }
}
