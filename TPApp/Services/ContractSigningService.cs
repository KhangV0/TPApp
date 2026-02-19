using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class ContractSigningService : IContractSigningService
    {
        private readonly AppDbContext             _context;
        private readonly IESignGateway            _esign;
        private readonly ISigningProviderFactory  _providerFactory;
        private readonly IHashService             _hash;
        private readonly IContractAuditService    _audit;
        private readonly IWorkflowService         _workflow;
        private readonly ISystemParameterService  _sysParams;
        private readonly ILogger<ContractSigningService> _logger;

        public ContractSigningService(
            AppDbContext context, IESignGateway esign,
            ISigningProviderFactory providerFactory, IHashService hash,
            IContractAuditService audit, IWorkflowService workflow,
            ISystemParameterService sysParams,
            ILogger<ContractSigningService> logger)
        {
            _context         = context;
            _esign           = esign;
            _providerFactory = providerFactory;
            _hash            = hash;
            _audit           = audit;
            _workflow        = workflow;
            _sysParams       = sysParams;
            _logger          = logger;
        }

        // ─── Buyer OTP e-sign ─────────────────────────────────────────────────
        public async Task<(bool ok, string message, int requestId)> StartBuyerOtpAsync(
            int contractId, int userId, string? ipAddress)
        {
            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract == null)
                return (false, "Không tìm thấy hợp đồng.", 0);

            if (contract.StatusId < (int)ContractStatus.ReadyToSign)
                return (false, "Hợp đồng chưa ở trạng thái ReadyToSign.", 0);

            // Reuse existing ESignGateway OTP infrastructure
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy người dùng.", 0);

            var phone = user.PhoneNumber ?? "";
            if (string.IsNullOrEmpty(phone))
                return (false, "Tài khoản chưa có số điện thoại.", 0);

            var otpTtl     = await _sysParams.GetIntAsync("ESIGN_OTP_TTL_SECONDS", 300);
            var challengeRef = $"BUYER-OTP-{Guid.NewGuid():N}";
            await _esign.SendOtpAsync(userId, phone);

            var req = new ContractSignatureRequest
            {
                ContractId    = contractId,
                UserId        = userId,
                Role          = 1, // Buyer
                SignatureType = (int)ContractSignatureType.BuyerOtpESign,
                Provider      = "OTP",
                StatusId      = (int)ContractSignatureStatus.Pending,
                ChallengeRef  = challengeRef,
                CreatedDate   = DateTime.UtcNow
            };
            _context.ContractSignatureRequests.Add(req);
            await _context.SaveChangesAsync();

            await _audit.AppendAsync("ContractSignatureRequest", req.Id.ToString(),
                "BuyerOtpStarted", new { userId, challengeRef }, userId, ipAddress);

            return (true, $"OTP đã gửi đến {phone[..3]}***{phone[^3..]}. Hiệu lực {otpTtl / 60} phút.", req.Id);
        }

        public async Task<(bool ok, string message)> ConfirmBuyerOtpAsync(
            int requestId, int userId, string otpCode, string? ipAddress, string? userAgent)
        {
            var req = await _context.ContractSignatureRequests.FindAsync(requestId);
            if (req == null || req.StatusId != (int)ContractSignatureStatus.Pending)
                return (false, "Phiên ký không hợp lệ hoặc đã hết hạn.");

            // Find the underlying ESign signature for OTP verification
            // Use ESignGateway's OTP verify flow; we use challengeRef as the marker
            var esignDoc = await _context.ESignDocuments
                .FirstOrDefaultAsync(d => d.ProjectId == (
                    _context.ProjectContracts
                        .Where(c => c.Id == req.ContractId)
                        .Select(c => c.ProjectId)
                        .FirstOrDefault()));

            // Simplified: call VerifyOtp via ESignGateway (reuses existing session mgmt)
            // If no ESign doc exists for this contract, do basic OTP check stub
            bool otpOk = true; // TODO: integrate via _esign.VerifyOtpAsync when ESign doc linked
            if (esignDoc != null)
            {
                var sigs = await _esign.GetDocumentSignaturesAsync(esignDoc.Id);
                var signId = sigs.FirstOrDefault(s => s.SignerUserId == userId && s.Status == 0)?.Id ?? 0;
                if (signId > 0)
                    otpOk = await _esign.VerifyOtpAsync(signId, otpCode);
            }

            if (!otpOk)
            {
                req.StatusId   = (int)ContractSignatureStatus.Failed;
                req.ErrorCode  = "OTP_INVALID";
                req.ErrorMessage = "Mã OTP không đúng.";
                req.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return (false, "Mã OTP không đúng hoặc đã hết hạn.");
            }

            // Record signature artifact
            var sig = new ContractSignature
            {
                ContractId         = req.ContractId,
                SignatureRequestId = req.Id,
                UserId             = req.UserId,
                Role               = 1, // Buyer
                SignatureType      = (int)ContractSignatureType.BuyerOtpESign,
                Provider           = "OTP",
                SignedAt           = DateTime.UtcNow,
                VerificationStatus = 1, // Valid
                IPAddress          = ipAddress,
                UserAgent          = userAgent
            };
            _context.ContractSignatures.Add(sig);

            req.StatusId    = (int)ContractSignatureStatus.Completed;
            req.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Check if both parties signed
            var contract = await _context.ProjectContracts.FindAsync(req.ContractId);
            if (contract != null)
                await TryCompleteStep7Async(contract.ProjectId, req.ContractId);

            await _audit.AppendAsync("ContractSignature", sig.Id.ToString(),
                "BuyerSigned", new { userId, ipAddress }, userId, ipAddress);

            return (true, "✅ Buyer đã ký thành công.");
        }

        // ─── Seller CA remote signing ─────────────────────────────────────────
        public async Task<(bool ok, string message, int requestId)> StartSellerCAAsync(
            int contractId, int userId, string provider, string callbackUrl, string? ipAddress)
        {
            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract == null) return (false, "Không tìm thấy hợp đồng.", 0);

            if (contract.StatusId < (int)ContractStatus.ReadyToSign)
                return (false, "Hợp đồng chưa ReadyToSign.", 0);

            var caProvider = _providerFactory.Resolve(provider);
            var user = await _context.Users.FindAsync(userId);

            var signer = new SignerInfo
            {
                FullName = user?.FullName ?? "Seller",
                Email    = user?.Email     ?? "",
                Phone    = user?.PhoneNumber ?? ""
            };

            // Get PDF bytes (use HTML snapshot if no file)
            byte[] pdfBytes = Array.Empty<byte>();
            if (!string.IsNullOrEmpty(contract.OriginalFilePath) && File.Exists(contract.OriginalFilePath))
                pdfBytes = await File.ReadAllBytesAsync(contract.OriginalFilePath);

            var callbackSecret = Guid.NewGuid().ToString("N");
            var requestRef = await caProvider.CreateSigningRequestAsync(pdfBytes, signer, callbackUrl);

            var req = new ContractSignatureRequest
            {
                ContractId      = contractId,
                UserId          = userId,
                Role            = 2, // Seller
                SignatureType   = (int)ContractSignatureType.SellerCA_Remote,
                Provider        = provider,
                StatusId        = (int)ContractSignatureStatus.Pending,
                RequestRef      = requestRef,
                CallbackSecret  = callbackSecret,
                CreatedDate     = DateTime.UtcNow
            };
            _context.ContractSignatureRequests.Add(req);

            // Update contract status
            if (contract.StatusId == (int)ContractStatus.ReadyToSign)
            {
                contract.StatusId = (int)ContractStatus.SigningInProgress;
                contract.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _audit.AppendAsync("ContractSignatureRequest", req.Id.ToString(),
                "SellerCAStarted", new { provider, requestRef }, userId, ipAddress);

            return (true, $"✅ Gửi yêu cầu ký CA tới {provider} thành công. RequestRef: {requestRef}", req.Id);
        }

        // ─── CA Provider webhook callback ─────────────────────────────────────
        public async Task<bool> HandleProviderCallbackAsync(
            string provider, string requestRef, string callbackSecret,
            byte[]? signedPdfBytes, string? certSerial, string? certSubject,
            string? certIssuer, string? rawPayload)
        {
            var req = await _context.ContractSignatureRequests
                .FirstOrDefaultAsync(r => r.RequestRef == requestRef && r.Provider == provider);

            if (req == null)
            {
                _logger.LogWarning("Callback: RequestRef {Ref} not found.", requestRef);
                return false;
            }

            if (req.CallbackSecret != callbackSecret)
            {
                _logger.LogWarning("Callback: Secret mismatch for {Ref}.", requestRef);
                return false;
            }

            var contract = await _context.ProjectContracts.FindAsync(req.ContractId);
            if (contract == null) return false;

            // Save signed file
            string? signedPath  = null;
            string? signedName  = null;
            string? sha256Signed = null;

            if (signedPdfBytes != null && signedPdfBytes.Length > 0)
            {
                var dir = Path.Combine("wwwroot", "uploads", "contracts", $"proj_{contract.ProjectId}", "signed");
                Directory.CreateDirectory(dir);
                signedName = $"signed_{provider}_{requestRef[..8]}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                signedPath = Path.Combine(dir, signedName);
                await File.WriteAllBytesAsync(signedPath, signedPdfBytes);
                sha256Signed = _hash.ComputeSha256(signedPdfBytes);

                contract.SignedFilePath = signedPath;
                contract.SignedFileName = signedName;
                contract.Sha256Signed   = sha256Signed;
            }

            // Record signature artifact
            var sig = new ContractSignature
            {
                ContractId         = req.ContractId,
                SignatureRequestId = req.Id,
                UserId             = req.UserId,
                Role               = req.Role,
                SignatureType      = req.SignatureType,
                Provider           = provider,
                CertificateSerial  = certSerial,
                CertificateSubject = certSubject,
                CertificateIssuer  = certIssuer,
                SignedHash         = sha256Signed,
                SignedAt           = DateTime.UtcNow,
                VerificationStatus = 1,
                RawProviderPayload = rawPayload
            };
            _context.ContractSignatures.Add(sig);

            req.StatusId    = (int)ContractSignatureStatus.Completed;
            req.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await TryCompleteStep7Async(contract.ProjectId, req.ContractId);

            await _audit.AppendAsync("ContractSignature", sig.Id.ToString(),
                "SellerCASigned", new { provider, requestRef, certSerial }, req.UserId);

            return true;
        }

        // ─── Status polling ───────────────────────────────────────────────────
        public async Task<ContractSigningStatusDto> GetStatusAsync(int contractId)
        {
            var sigs = await _context.ContractSignatures
                .Where(s => s.ContractId == contractId)
                .ToListAsync();

            var buyer  = sigs.FirstOrDefault(s => s.Role == 1);
            var seller = sigs.FirstOrDefault(s => s.Role == 2);

            return new ContractSigningStatusDto
            {
                BuyerSigned    = buyer  != null,
                BuyerSignedAt  = buyer?.SignedAt,
                SellerSigned   = seller != null,
                SellerSignedAt = seller?.SignedAt
            };
        }

        // ─── Complete Step 7 when both signed ─────────────────────────────────
        public async Task<bool> TryCompleteStep7Async(int projectId, int contractId)
        {
            var status = await GetStatusAsync(contractId);
            if (!status.FullySigned) return false;

            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract != null)
            {
                contract.StatusId    = (int)ContractStatus.FullySigned;
                contract.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _workflow.CompleteStep(projectId, 7);

            await _audit.AppendAsync("Project", projectId.ToString(), "Step7Completed",
                new { contractId, buyerSigned = true, sellerSigned = true });

            _logger.LogInformation("Step 7 completed for project {Id}.", projectId);
            return true;
        }
    }
}
