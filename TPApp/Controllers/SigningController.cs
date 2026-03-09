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
        private readonly Services.PdfSigningService _pdfSigner;
        private readonly Services.HtmlToPdfService  _htmlToPdf;
        private readonly IWebHostEnvironment        _env;

        public SigningController(
            AppDbContext context, UserManager<ApplicationUser> userManager,
            IContractSigningService signing, IContractAuditService audit,
            ISystemParameterService sysParams, ILogger<SigningController> logger,
            Services.INotificationQueueService notifQueue,
            Services.PdfSigningService pdfSigner,
            Services.HtmlToPdfService htmlToPdf,
            IWebHostEnvironment env)
        {
            _context    = context;
            _userManager = userManager;
            _signing    = signing;
            _audit      = audit;
            _sysParams  = sysParams;
            _logger     = logger;
            _notifQueue = notifQueue;
            _pdfSigner  = pdfSigner;
            _htmlToPdf  = htmlToPdf;
            _env        = env;
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

        // ─── POST /Signing/BuyerCAStart  (AJAX – CA digital sig for enterprise) ──
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> BuyerCAStart([FromBody] SignContractDto dto)
        {
            try
            {
                var userId   = GetUserId();
                var ip       = HttpContext.Connection.RemoteIpAddress?.ToString();
                var provider = await _sysParams.GetAsync("SIGNING_PROVIDER_DEFAULT") ?? "VNPT";
                var callbackUrl = $"{Request.Scheme}://{Request.Host}/Signing/Callback/{provider}";

                var (ok, msg, reqId) = await _signing.StartBuyerCAAsync(dto.ContractId, userId, provider, callbackUrl, ip);
                return Json(new { success = ok, message = msg, requestId = reqId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BuyerCAStart error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── POST /Signing/SellerCAStart  (AJAX – CA digital sig for enterprise) ──
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SellerCAStart([FromBody] SignContractDto dto)
        {
            try
            {
                var userId   = GetUserId();
                var ip       = HttpContext.Connection.RemoteIpAddress?.ToString();
                var provider = await _sysParams.GetAsync("SIGNING_PROVIDER_DEFAULT") ?? "VNPT";
                var callbackUrl = $"{Request.Scheme}://{Request.Host}/Signing/Callback/{provider}";

                var (ok, msg, reqId) = await _signing.StartSellerCAAsync(dto.ContractId, userId, provider, callbackUrl, ip);
                return Json(new { success = ok, message = msg, requestId = reqId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SellerCAStart error");
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

        // ─── POST /Signing/MockCallback  (dev-only: simulates CA callback) ───
        [HttpPost, AllowAnonymous, IgnoreAntiforgeryToken]
        public async Task<IActionResult> MockCallback([FromBody] ProviderCallbackDto dto)
        {
            try
            {
                _logger.LogWarning("🧪 MockCallback received for ref={Ref}", dto.RequestRef);

                // Lookup the request to get the actual CallbackSecret
                var req = await _context.ContractSignatureRequests
                    .FirstOrDefaultAsync(r => r.RequestRef == dto.RequestRef);

                if (req == null)
                    return BadRequest(new { status = "request_not_found" });

                // Use the real secret stored in DB (bypasses validation)
                bool ok = await _signing.HandleProviderCallbackAsync(
                    req.Provider ?? "VNPT",
                    dto.RequestRef,
                    req.CallbackSecret ?? "",   // use stored secret so validation passes
                    null,                        // no signed PDF in stub
                    dto.CertSerial  ?? "STUB-CERT-001",
                    dto.CertSubject ?? $"CN=Stub Signer",
                    dto.CertIssuer  ?? "CN=TechPort Stub CA",
                    null);

                if (ok)
                {
                    // Send notification for successful signing
                    var contract = await _context.ProjectContracts.FindAsync(req.ContractId);
                    if (contract != null)
                    {
                        var proj = await _context.Projects.FindAsync(contract.ProjectId);
                        if (proj != null)
                        {
                            var roleName = req.Role == 1 ? "Bên A (Đơn vị mua)" : "Bên B (Đơn vị cung cấp)";
                            var notifMsg = $"{roleName} đã ký số hợp đồng thành công (CA).";
                            if (proj.CreatedBy.HasValue)
                                await _notifQueue.QueueAsync(proj.CreatedBy.Value, contract.ProjectId, "✍️ Bước 7: Ký số CA", notifMsg);
                            if (proj.SelectedSellerId.HasValue)
                                await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, contract.ProjectId, "✍️ Bước 7: Ký số CA", notifMsg);
                        }
                    }
                }

                return ok ? Ok(new { status = "ok" }) : BadRequest(new { status = "rejected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MockCallback error");
                return StatusCode(500);
            }
        }

        // ─── GET /Signing/ContractHash?contractId=  (get PDF hash for USB Token) ──
        [HttpGet]
        public async Task<IActionResult> ContractHash(int contractId)
        {
            try
            {
                var contract = await _context.ProjectContracts.FindAsync(contractId);
                if (contract == null)
                    return Json(new { success = false, message = "Hợp đồng không tồn tại." });

                // ─ Priority 1: uploaded PDF file
                byte[] pdfBytes = Array.Empty<byte>();
                string pdfSource = "file";

                if (!string.IsNullOrEmpty(contract.OriginalFilePath) &&
                    System.IO.File.Exists(contract.OriginalFilePath))
                {
                    pdfBytes = await System.IO.File.ReadAllBytesAsync(contract.OriginalFilePath);
                }
                // ─ Priority 2: generate from HtmlContent
                else if (!string.IsNullOrEmpty(contract.HtmlContent))
                {
                    _logger.LogInformation("ContractHash: no file found, generating PDF from HtmlContent");
                    pdfBytes = _htmlToPdf.Convert(contract.HtmlContent, contract.Title);
                    pdfSource = "generated";

                    // Cache the generated PDF so signing uses the same bytes
                    var dir = Path.Combine(_env.WebRootPath, "uploads", "contracts", $"proj_{contract.ProjectId}");
                    Directory.CreateDirectory(dir);
                    var genName = $"contract_{contract.Id}_generated.pdf";
                    var genPath = Path.Combine(dir, genName);
                    await System.IO.File.WriteAllBytesAsync(genPath, pdfBytes);

                    // Save path so USBTokenSign can find it
                    contract.OriginalFilePath = genPath;
                    contract.OriginalFileName = genName;
                    await _context.SaveChangesAsync();
                }

                if (pdfBytes.Length == 0)
                    return Json(new { success = false, message = "Không có nội dung hợp đồng để tạo hash." });

                // SHA-256 hash
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hashBytes  = sha256.ComputeHash(pdfBytes);
                var hashBase64 = Convert.ToBase64String(hashBytes);
                var hashHex    = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return Json(new
                {
                    success = true,
                    contractId,
                    hashBase64,
                    hashHex,
                    fileSize  = pdfBytes.Length,
                    algorithm = "SHA-256",
                    pdfSource         // "file" | "generated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ContractHash error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── GET /Signing/DownloadSignedPdf?contractId=&mode=view|download ────
        [HttpGet]
        public async Task<IActionResult> DownloadSignedPdf(int contractId, string mode = "download")
        {
            try
            {
                var userId   = GetUserId();
                var contract = await _context.ProjectContracts.FindAsync(contractId);
                if (contract == null) return NotFound("Hợp đồng không tồn tại.");

                // Only buyer or seller can access
                var proj = await _context.Projects.FindAsync(contract.ProjectId);
                bool isBuyer  = proj?.CreatedBy       == userId;
                bool isSeller = proj?.SelectedSellerId == userId;
                if (!isBuyer && !isSeller) return Forbid();

                if (string.IsNullOrEmpty(contract.SignedFilePath) ||
                    !System.IO.File.Exists(contract.SignedFilePath))
                {
                    return NotFound("File đã ký chưa có. Vui lòng hoàn tất ký số trước.");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(contract.SignedFilePath);
                var outName   = $"HopDong_DaKy_{contractId}_{DateTime.Now:yyyyMMdd}.pdf";

                _logger.LogInformation("Signed PDF {Mode}: contractId={Id}, userId={UserId}",
                    mode, contractId, userId);

                // mode=view  → open inline in browser (PDF viewer)
                // mode=download → force-download
                if (mode == "view")
                    return File(fileBytes, "application/pdf");   // inline (no Content-Disposition filename)

                return File(fileBytes, "application/pdf", outName); // attachment
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DownloadSignedPdf error");
                return StatusCode(500, ex.Message);
            }
        }

        // ─── GET /Signing/SignedPdfInfo?contractId= — check if signed PDF exists ──
        [HttpGet]
        public async Task<IActionResult> SignedPdfInfo(int contractId)
        {
            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract == null)
                return Json(new { exists = false });

            bool exists = !string.IsNullOrEmpty(contract.SignedFilePath) &&
                          System.IO.File.Exists(contract.SignedFilePath);

            return Json(new
            {
                exists,
                fileName = contract.SignedFileName,
                sha256   = contract.Sha256Signed
            });
        }

        // ─── POST /Signing/USBTokenSign (receive signed hash from USB Token agent) ──
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> USBTokenSign([FromBody] USBTokenSignDto dto)
        {
            try
            {
                var userId = GetUserId();
                var ip     = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua     = Request.Headers["User-Agent"].ToString();

                var contract = await _context.ProjectContracts.FindAsync(dto.ContractId);
                if (contract == null)
                    return Json(new { success = false, message = "Hợp đồng không tồn tại." });

                var proj = await _context.Projects.FindAsync(contract.ProjectId);
                bool isBuyer  = proj?.CreatedBy == userId;
                bool isSeller = proj?.SelectedSellerId == userId;
                int role = isBuyer ? 1 : isSeller ? 2 : 0;

                if (role == 0)
                    return Json(new { success = false, message = "Bạn không có quyền ký hợp đồng này." });

                var sigType = role == 1
                    ? (int)TPApp.Enums.ContractSignatureType.BuyerUSBToken_Local
                    : (int)TPApp.Enums.ContractSignatureType.SellerUSBToken_Local;

                // Save signature request
                var req = new TPApp.Entities.ContractSignatureRequest
                {
                    ContractId    = dto.ContractId,
                    UserId        = userId,
                    Role          = role,
                    SignatureType = sigType,
                    Provider      = "USBToken",
                    StatusId      = (int)TPApp.Enums.ContractSignatureStatus.Completed,
                    RequestRef    = $"USB-{Guid.NewGuid():N}",
                    CreatedDate   = DateTime.UtcNow,
                    UpdatedDate   = DateTime.UtcNow
                };
                _context.ContractSignatureRequests.Add(req);
                await _context.SaveChangesAsync();

                // ─── Handle signature storage ─── 
                string? signedPath = null;
                string? sha256Signed = null;
                string? signatureHex = null;

                var sigDir = Path.Combine(_env.WebRootPath, "uploads", "contracts", $"proj_{contract.ProjectId}", "signed");
                Directory.CreateDirectory(sigDir);

                // New flow: SignatureBase64 + CertificateBase64 (from LocalSigner)
                if (!string.IsNullOrEmpty(dto.SignatureBase64))
                {
                    var sigBytes = Convert.FromBase64String(dto.SignatureBase64);
                    signatureHex = BitConverter.ToString(sigBytes).Replace("-", "").ToLowerInvariant();

                    // Save detached signature file (.sig)
                    var sigFileName = $"sig_usb_{role}_{DateTime.UtcNow:yyyyMMddHHmmss}.sig";
                    await System.IO.File.WriteAllBytesAsync(Path.Combine(sigDir, sigFileName), sigBytes);

                    // Save certificate file (.cer)
                    byte[]? certBytes = null;
                    if (!string.IsNullOrEmpty(dto.CertificateBase64))
                    {
                        certBytes = Convert.FromBase64String(dto.CertificateBase64);
                        var certFileName = $"cert_usb_{role}_{DateTime.UtcNow:yyyyMMddHHmmss}.cer";
                        await System.IO.File.WriteAllBytesAsync(Path.Combine(sigDir, certFileName), certBytes);
                    }

                    // Embed visible signature panel into PDF
                    if (!string.IsNullOrEmpty(contract.OriginalFilePath) &&
                        System.IO.File.Exists(contract.OriginalFilePath))
                    {
                        try
                        {
                            signedPath = await _pdfSigner.EmbedVisibleSignatureAsync(
                                sourcePdfPath:    contract.OriginalFilePath,
                                signatureBytes:   sigBytes,
                                certificateBytes: certBytes ?? [],
                                certSubject:      dto.CertSubject ?? "",
                                certIssuer:       dto.CertIssuer  ?? "",
                                certSerial:       dto.CertSerial  ?? "",
                                role:             role,
                                projectId:        contract.ProjectId);

                            contract.SignedFilePath = signedPath;
                            contract.SignedFileName = Path.GetFileName(signedPath);

                            using var sha = System.Security.Cryptography.SHA256.Create();
                            var signedBytes2 = await System.IO.File.ReadAllBytesAsync(signedPath);
                            contract.Sha256Signed = BitConverter.ToString(sha.ComputeHash(signedBytes2)).Replace("-", "").ToLowerInvariant();

                            _logger.LogInformation("Visible signature embedded: {Path}", signedPath);
                        }
                        catch (Exception pdfEx)
                        {
                            _logger.LogError(pdfEx, "PDF signature embedding failed — signature still recorded");
                        }
                    }
                }
                // Legacy flow: pre-signed PDF
                else if (!string.IsNullOrEmpty(dto.SignedPdfBase64))
                {
                    var signedBytes = Convert.FromBase64String(dto.SignedPdfBase64);
                    var signedName = $"signed_usb_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                    signedPath = Path.Combine(sigDir, signedName);
                    await System.IO.File.WriteAllBytesAsync(signedPath, signedBytes);

                    using var sha = System.Security.Cryptography.SHA256.Create();
                    sha256Signed = BitConverter.ToString(sha.ComputeHash(signedBytes)).Replace("-", "").ToLowerInvariant();

                    contract.SignedFilePath = signedPath;
                    contract.SignedFileName = signedName;
                    contract.Sha256Signed = sha256Signed;
                }

                // Record signature artifact
                var sig = new TPApp.Entities.ContractSignature
                {
                    ContractId         = dto.ContractId,
                    SignatureRequestId = req.Id,
                    UserId             = userId,
                    Role               = role,
                    SignatureType      = sigType,
                    Provider           = "USBToken",
                    CertificateSerial  = dto.CertSerial?[..Math.Min(200, dto.CertSerial?.Length ?? 0)],
                    CertificateSubject = dto.CertSubject?[..Math.Min(500, dto.CertSubject?.Length ?? 0)],
                    CertificateIssuer  = dto.CertIssuer?[..Math.Min(500, dto.CertIssuer?.Length ?? 0)],
                    // SignedHash stores first 128 chars of hex (full sig stored in .sig file)
                    SignedHash         = (signatureHex ?? dto.SignedHash ?? sha256Signed)?[..Math.Min(128, (signatureHex ?? dto.SignedHash ?? sha256Signed)?.Length ?? 0)],
                    SignedAt           = DateTime.UtcNow,
                    VerificationStatus = 1,
                    IPAddress          = ip?[..Math.Min(100, ip?.Length ?? 0)],
                    UserAgent          = ua?[..Math.Min(400, ua?.Length ?? 0)]
                };
                _context.ContractSignatures.Add(sig);

                // Update contract status
                if (contract.StatusId == (int)TPApp.Enums.ContractStatus.ReadyToSign)
                {
                    contract.StatusId = (int)TPApp.Enums.ContractStatus.SigningInProgress;
                    contract.ModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Check if both signed
                await _signing.TryCompleteStep7Async(contract.ProjectId, dto.ContractId);

                await _audit.AppendAsync("ContractSignature", sig.Id.ToString(),
                    "USBTokenSigned", new { userId, role, certSerial = dto.CertSerial, ip }, userId, ip);

                var roleName = role == 1 ? "Buyer" : "Seller";
                _logger.LogInformation("USB Token signed by {Role} (userId={UserId}) for contract {ContractId}",
                    roleName, userId, dto.ContractId);

                return Json(new { success = true, message = $"✅ {roleName} đã ký USB Token thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "USBTokenSign error");
                return Json(new { success = false, message = ex.Message });
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
    public class USBTokenSignDto
    {
        public int ContractId { get; set; }
        /// <summary>Base64 RSA signature from LocalSigner agent</summary>
        public string? SignatureBase64 { get; set; }
        /// <summary>Base64 X.509 certificate from USB Token</summary>
        public string? CertificateBase64 { get; set; }
        /// <summary>Legacy: Base64 of a pre-signed PDF</summary>
        public string? SignedPdfBase64 { get; set; }
        /// <summary>Legacy: Hex signed hash</summary>
        public string? SignedHash { get; set; }
        /// <summary>Certificate serial number from USB Token</summary>
        public string? CertSerial { get; set; }
        /// <summary>Certificate subject (CN=...)</summary>
        public string? CertSubject { get; set; }
        /// <summary>Certificate issuer (CN=...)</summary>
        public string? CertIssuer { get; set; }
    }
}

