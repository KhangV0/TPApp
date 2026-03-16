using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IOtpEmailService         _otpEmail;
        private readonly IConfiguration            _configuration;
        private readonly ILogger<ContractSigningService> _logger;

        public ContractSigningService(
            AppDbContext context, IESignGateway esign,
            ISigningProviderFactory providerFactory, IHashService hash,
            IContractAuditService audit, IWorkflowService workflow,
            ISystemParameterService sysParams, IOtpEmailService otpEmail,
            IConfiguration configuration,
            ILogger<ContractSigningService> logger)
        {
            _context         = context;
            _esign           = esign;
            _providerFactory = providerFactory;
            _hash            = hash;
            _audit           = audit;
            _workflow        = workflow;
            _sysParams       = sysParams;
            _otpEmail        = otpEmail;
            _configuration   = configuration;
            _logger          = logger;
        }

        // ─── Buyer OTP e-sign ─────────────────────────────────────────────────
        public async Task<(bool ok, string message, int requestId)> StartBuyerOtpAsync(
            int contractId, int userId, string? ipAddress)
        {
            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract == null)
                return (false, "Không tìm thấy hợp đồng.", 0);

            if (!_configuration.GetValue<bool>("Signing:TestMode") &&
                contract.StatusId < (int)ContractStatus.ReadyToSign)
                return (false, "Hợp đồng chưa ở trạng thái ReadyToSign.", 0);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy người dùng.", 0);

            var email    = user.Email ?? "";
            var phone    = user.PhoneNumber ?? "";
            var fullName = user.FullName ?? user.UserName ?? "Người dùng";

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone))
                return (false, "Tài khoản chưa có email hoặc số điện thoại.", 0);

            var otpTtl = await _sysParams.GetIntAsync("ESIGN_OTP_TTL_SECONDS", 300);

            // Generate OTP directly (always available, regardless of TestMode)
            var otp = new Random().Next(100000, 999999).ToString();
            var otpHash = HashOtp(otp);
            var challengeRef = $"BUYER-OTP-{otpHash}"; // store hash for verification

            _logger.LogInformation("Step7 Buyer OTP for user {UserId}: {Otp}", userId, otp);

            // Always send OTP via email
            if (!string.IsNullOrEmpty(email))
            {
                await _otpEmail.SendOtpAsync(email, fullName, otp, "Buyer", contract.ProjectId);
            }

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

            var maskedEmail = !string.IsNullOrEmpty(email) && email.Length > 3
                ? email[..3] + "***" + email[email.IndexOf('@')..]
                : email;
            return (true, $"OTP đã gửi đến {maskedEmail}. Hiệu lực {otpTtl / 60} phút.", req.Id);
        }

        public async Task<(bool ok, string message)> ConfirmBuyerOtpAsync(
            int requestId, int userId, string otpCode, string? ipAddress, string? userAgent)
        {
            var req = await _context.ContractSignatureRequests.FindAsync(requestId);
            if (req == null || req.StatusId != (int)ContractSignatureStatus.Pending)
                return (false, "Phiên ký không hợp lệ hoặc đã hết hạn.");

            // Verify OTP
            var isTestMode = _configuration.GetValue<bool>("ESign:TestMode", true);
            bool otpOk;
            if (isTestMode)
            {
                // TestMode: accept any 6-digit code
                otpOk = otpCode.Length == 6 && int.TryParse(otpCode, out _);
            }
            else
            {
                // Production: verify against stored hash in ChallengeRef
                var expectedHash = req.ChallengeRef?.Replace("BUYER-OTP-", "") ?? "";
                otpOk = HashOtp(otpCode) == expectedHash;
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

            // Check if both parties signed — skip khi TestMode
            var contract2 = await _context.ProjectContracts.FindAsync(req.ContractId);
            var testMode2 = _configuration.GetValue<bool>("Signing:TestMode");
            if (contract2 != null && !testMode2)
                await TryCompleteStep7Async(contract2.ProjectId, req.ContractId);
            else if (testMode2)
                _logger.LogWarning("[TestMode] BuyerOTP signed — TryCompleteStep7 skipped");

            await _audit.AppendAsync("ContractSignature", sig.Id.ToString(),
                "BuyerSigned", new { userId, ipAddress }, userId, ipAddress);

            return (true, "✅ Buyer đã ký thành công.");
        }

        // ─── Seller OTP e-sign ────────────────────────────────────────────────
        public async Task<(bool ok, string message, int requestId)> StartSellerOtpAsync(
            int contractId, int userId, string? ipAddress)
        {
            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract == null)
                return (false, "Không tìm thấy hợp đồng.", 0);

            if (!_configuration.GetValue<bool>("Signing:TestMode") &&
                contract.StatusId < (int)ContractStatus.ReadyToSign)
                return (false, "Hợp đồng chưa ở trạng thái ReadyToSign.", 0);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy người dùng.", 0);

            var email    = user.Email ?? "";
            var phone    = user.PhoneNumber ?? "";
            var fullName = user.FullName ?? user.UserName ?? "Người dùng";

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone))
                return (false, "Tài khoản chưa có email hoặc số điện thoại.", 0);

            var otpTtl = await _sysParams.GetIntAsync("ESIGN_OTP_TTL_SECONDS", 300);

            var otp = new Random().Next(100000, 999999).ToString();
            var otpHash = HashOtp(otp);
            var challengeRef = $"SELLER-OTP-{otpHash}";

            _logger.LogInformation("Step7 Seller OTP for user {UserId}: {Otp}", userId, otp);

            if (!string.IsNullOrEmpty(email))
            {
                await _otpEmail.SendOtpAsync(email, fullName, otp, "Seller", contract.ProjectId);
            }

            var req = new ContractSignatureRequest
            {
                ContractId    = contractId,
                UserId        = userId,
                Role          = 2, // Seller
                SignatureType = (int)ContractSignatureType.SellerOtpESign,
                Provider      = "OTP",
                StatusId      = (int)ContractSignatureStatus.Pending,
                ChallengeRef  = challengeRef,
                CreatedDate   = DateTime.UtcNow
            };
            _context.ContractSignatureRequests.Add(req);
            await _context.SaveChangesAsync();

            await _audit.AppendAsync("ContractSignatureRequest", req.Id.ToString(),
                "SellerOtpStarted", new { userId, challengeRef }, userId, ipAddress);

            var maskedEmail = !string.IsNullOrEmpty(email) && email.Length > 3
                ? email[..3] + "***" + email[email.IndexOf('@')..]
                : email;
            return (true, $"OTP đã gửi đến {maskedEmail}. Hiệu lực {otpTtl / 60} phút.", req.Id);
        }

        public async Task<(bool ok, string message)> ConfirmSellerOtpAsync(
            int requestId, int userId, string otpCode, string? ipAddress, string? userAgent)
        {
            var req = await _context.ContractSignatureRequests.FindAsync(requestId);
            if (req == null || req.StatusId != (int)ContractSignatureStatus.Pending)
                return (false, "Phiên ký không hợp lệ hoặc đã hết hạn.");

            var isTestMode = _configuration.GetValue<bool>("ESign:TestMode", true);
            bool otpOk;
            if (isTestMode)
            {
                otpOk = otpCode.Length == 6 && int.TryParse(otpCode, out _);
            }
            else
            {
                var expectedHash = req.ChallengeRef?.Replace("SELLER-OTP-", "") ?? "";
                otpOk = HashOtp(otpCode) == expectedHash;
            }

            if (!otpOk)
            {
                req.StatusId     = (int)ContractSignatureStatus.Failed;
                req.ErrorCode    = "OTP_INVALID";
                req.ErrorMessage = "Mã OTP không đúng.";
                req.UpdatedDate  = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return (false, "Mã OTP không đúng hoặc đã hết hạn.");
            }

            var sig = new ContractSignature
            {
                ContractId         = req.ContractId,
                SignatureRequestId = req.Id,
                UserId             = req.UserId,
                Role               = 2, // Seller
                SignatureType      = (int)ContractSignatureType.SellerOtpESign,
                Provider           = "OTP",
                SignedAt           = DateTime.UtcNow,
                VerificationStatus = 1,
                IPAddress          = ipAddress,
                UserAgent          = userAgent
            };
            _context.ContractSignatures.Add(sig);

            req.StatusId    = (int)ContractSignatureStatus.Completed;
            req.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Check if both parties signed — skip khi TestMode
            var contract3 = await _context.ProjectContracts.FindAsync(req.ContractId);
            var testMode3 = _configuration.GetValue<bool>("Signing:TestMode");
            if (contract3 != null && !testMode3)
                await TryCompleteStep7Async(contract3.ProjectId, req.ContractId);
            else if (testMode3)
                _logger.LogWarning("[TestMode] SellerOTP signed — TryCompleteStep7 skipped");

            await _audit.AppendAsync("ContractSignature", sig.Id.ToString(),
                "SellerSigned", new { userId, ipAddress }, userId, ipAddress);

            return (true, "✅ Seller đã ký thành công.");
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

        // ─── Buyer CA remote signing (enterprise) ─────────────────────────────
        public async Task<(bool ok, string message, int requestId)> StartBuyerCAAsync(
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
                FullName = user?.FullName ?? "Buyer",
                Email    = user?.Email     ?? "",
                Phone    = user?.PhoneNumber ?? ""
            };

            byte[] pdfBytes = Array.Empty<byte>();
            if (!string.IsNullOrEmpty(contract.OriginalFilePath) && File.Exists(contract.OriginalFilePath))
                pdfBytes = await File.ReadAllBytesAsync(contract.OriginalFilePath);

            var callbackSecret = Guid.NewGuid().ToString("N");
            var requestRef = await caProvider.CreateSigningRequestAsync(pdfBytes, signer, callbackUrl);

            var req = new ContractSignatureRequest
            {
                ContractId      = contractId,
                UserId          = userId,
                Role            = 1, // Buyer
                SignatureType   = (int)ContractSignatureType.BuyerCA_Remote,
                Provider        = provider,
                StatusId        = (int)ContractSignatureStatus.Pending,
                RequestRef      = requestRef,
                CallbackSecret  = callbackSecret,
                CreatedDate     = DateTime.UtcNow
            };
            _context.ContractSignatureRequests.Add(req);

            if (contract.StatusId == (int)ContractStatus.ReadyToSign)
            {
                contract.StatusId = (int)ContractStatus.SigningInProgress;
                contract.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _audit.AppendAsync("ContractSignatureRequest", req.Id.ToString(),
                "BuyerCAStarted", new { provider, requestRef }, userId, ipAddress);

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

        // ─── Helper: hash OTP for storage/comparison ──────────────────────────
        private static string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
