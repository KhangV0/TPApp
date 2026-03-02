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
    public class SigningController : Controller
    {
        private readonly AppDbContext              _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContractSigningService   _signing;
        private readonly IContractAuditService     _audit;
        private readonly ISystemParameterService   _sysParams;
        private readonly ILogger<SigningController> _logger;
        private readonly Services.INotificationQueueService _notifQueue;

        public SigningController(
            AppDbContext context, UserManager<ApplicationUser> userManager,
            IContractSigningService signing, IContractAuditService audit,
            ISystemParameterService sysParams, ILogger<SigningController> logger,
            Services.INotificationQueueService notifQueue)
        {
            _context    = context;
            _userManager = userManager;
            _signing    = signing;
            _audit      = audit;
            _sysParams  = sysParams;
            _logger     = logger;
            _notifQueue = notifQueue;
        }

        private int GetUserId()
        {
            var s = _userManager.GetUserId(User);
            return int.TryParse(s, out int id) ? id : 0;
        }

        // ─── GET /Signing/Index?projectId= ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(int projectId)
        {
            var userId  = GetUserId();
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            bool isBuyer  = project.CreatedBy == userId;
            bool isSeller = project.SelectedSellerId == userId;
            if (!isBuyer && !isSeller) return Forbid();

            var contract = await _context.ProjectContracts
                .Where(c => c.ProjectId == projectId && c.IsActive)
                .OrderByDescending(c => c.VersionNumber)
                .FirstOrDefaultAsync();

            if (contract == null || contract.StatusId < (int)ContractStatus.ReadyToSign)
            {
                TempData["Error"] = "Hợp đồng chưa ở trạng thái ReadyToSign.";
                return RedirectToAction("Index", "Contract", new { projectId });
            }

            var status    = await _signing.GetStatusAsync(contract.Id);
            var provider  = await _sysParams.GetAsync("SIGNING_PROVIDER_DEFAULT") ?? "VNPT";
            var auditLogs = await _context.ContractAuditLogs
                .Where(l => l.EntityId == contract.Id.ToString())
                .OrderByDescending(l => l.CreatedDate)
                .Take(20)
                .ToListAsync();

            ViewBag.Project   = project;
            ViewBag.ProjectId = projectId;
            ViewBag.IsBuyer   = isBuyer;
            ViewBag.IsSeller  = isSeller;
            ViewBag.Status    = status;
            ViewBag.Provider  = provider;
            ViewBag.AuditLogs = auditLogs;

            return View(contract);
        }

        // ─── POST /Signing/BuyerStart  (AJAX) ─────────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> BuyerStart([FromBody] SignContractDto dto)
        {
            try
            {
                var userId = GetUserId();
                var ip     = HttpContext.Connection.RemoteIpAddress?.ToString();
                var (ok, msg, reqId) = await _signing.StartBuyerOtpAsync(dto.ContractId, userId, ip);
                return Json(new { success = ok, message = msg, requestId = reqId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BuyerStart error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── POST /Signing/BuyerConfirm  (AJAX) ───────────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> BuyerConfirm([FromBody] ConfirmOtpDto dto)
        {
            try
            {
                var userId = GetUserId();
                var ip     = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua     = Request.Headers["User-Agent"].ToString();
                var (ok, msg) = await _signing.ConfirmBuyerOtpAsync(dto.RequestId, userId, dto.OtpCode, ip, ua);

                // Notify on successful sign
                if (ok)
                {
                    var contract = await _context.ProjectContracts.FindAsync(dto.ContractId);
                    if (contract?.ProjectId != null)
                    {
                        var proj = await _context.Projects.FindAsync(contract.ProjectId);
                        if (proj != null)
                        {
                            var notifMsg = "Bên A (Đơn vị mua) đã ký số hợp đồng thành công.";
                            if (proj.CreatedBy.HasValue)
                                await _notifQueue.QueueAsync(proj.CreatedBy.Value, contract.ProjectId, "✍️ Bước 7: Ký số", notifMsg);
                            if (proj.SelectedSellerId.HasValue)
                                await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, contract.ProjectId, "✍️ Bước 7: Ký số", notifMsg);
                        }
                    }
                }

                return Json(new { success = ok, message = msg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BuyerConfirm error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── POST /Signing/SellerStart  (AJAX – OTP) ──────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SellerStart([FromBody] SignContractDto dto)
        {
            try
            {
                var userId = GetUserId();
                var ip     = HttpContext.Connection.RemoteIpAddress?.ToString();
                var (ok, msg, reqId) = await _signing.StartSellerOtpAsync(dto.ContractId, userId, ip);
                return Json(new { success = ok, message = msg, requestId = reqId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SellerStart error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── POST /Signing/SellerConfirm  (AJAX – OTP) ──────────────────────────
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SellerConfirm([FromBody] ConfirmOtpDto dto)
        {
            try
            {
                var userId = GetUserId();
                var ip     = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua     = Request.Headers["User-Agent"].ToString();
                var (ok, msg) = await _signing.ConfirmSellerOtpAsync(dto.RequestId, userId, dto.OtpCode, ip, ua);

                // Notify on successful sign
                if (ok)
                {
                    var contract = await _context.ProjectContracts.FindAsync(dto.ContractId);
                    if (contract?.ProjectId != null)
                    {
                        var proj = await _context.Projects.FindAsync(contract.ProjectId);
                        if (proj != null)
                        {
                            var notifMsg = "Bên B (Đơn vị cung cấp) đã ký số hợp đồng thành công.";
                            if (proj.CreatedBy.HasValue)
                                await _notifQueue.QueueAsync(proj.CreatedBy.Value, contract.ProjectId, "✍️ Bước 7: Ký số", notifMsg);
                            if (proj.SelectedSellerId.HasValue)
                                await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, contract.ProjectId, "✍️ Bước 7: Ký số", notifMsg);
                        }
                    }
                }

                return Json(new { success = ok, message = msg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SellerConfirm error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── GET /Signing/Status?contractId=  (AJAX poll) ─────────────────────
        [HttpGet]
        public async Task<IActionResult> Status(int contractId)
        {
            var status = await _signing.GetStatusAsync(contractId);
            return Json(new
            {
                buyerSigned    = status.BuyerSigned,
                buyerSignedAt  = status.BuyerSignedAt?.ToString("dd/MM/yyyy HH:mm"),
                sellerSigned   = status.SellerSigned,
                sellerSignedAt = status.SellerSignedAt?.ToString("dd/MM/yyyy HH:mm"),
                fullySigned    = status.FullySigned
            });
        }

        // ─── POST /Signing/Callback/{provider}  (CA webhook – no auth) ────────
        [HttpPost, AllowAnonymous, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Callback(string provider, [FromBody] ProviderCallbackDto dto)
        {
            try
            {
                _logger.LogInformation("CA Callback received from {Provider}, ref={Ref}", provider, dto.RequestRef);

                byte[]? signedBytes = null;
                if (!string.IsNullOrEmpty(dto.SignedPdfBase64))
                    signedBytes = Convert.FromBase64String(dto.SignedPdfBase64);

                bool ok = await _signing.HandleProviderCallbackAsync(
                    provider, dto.RequestRef, dto.CallbackSecret,
                    signedBytes, dto.CertSerial, dto.CertSubject, dto.CertIssuer, dto.RawPayload);

                return ok ? Ok(new { status = "ok" }) : BadRequest(new { status = "rejected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback error from provider {P}", provider);
                return StatusCode(500);
            }
        }
    }

    // ─── DTOs ─────────────────────────────────────────────────────────────────
    public class SignContractDto
    {
        public int ContractId { get; set; }
        public string Channel { get; set; } = "email";
        public string PhoneNumber { get; set; } = "";
    }
    public class ConfirmOtpDto
    {
        public int ContractId { get; set; }
        public int RequestId { get; set; }  // kept for backward compat
        public string OtpCode { get; set; } = "";
    }
    public class ProviderCallbackDto
    {
        public string  RequestRef      { get; set; } = "";
        public string  CallbackSecret  { get; set; } = "";
        public string? SignedPdfBase64 { get; set; }
        public string? CertSerial      { get; set; }
        public string? CertSubject     { get; set; }
        public string? CertIssuer      { get; set; }
        public string? RawPayload      { get; set; }
    }
}
