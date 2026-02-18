using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TPApp.Data;
using TPApp.Entities.ESign;
using TPApp.Interfaces;

namespace TPApp.Services
{
    /// <summary>
    /// Implementation of E-Sign gateway for document signing with OTP verification
    /// </summary>
    public class ESignGateway : IESignGateway
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ESignGateway> _logger;
        private readonly IConfiguration _configuration;

        public ESignGateway(AppDbContext context, ILogger<ESignGateway> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> IsProjectNdaSignedAsync(int projectId, int buyerUserId)
        {
            var ndaDoc = await GetProjectNdaAsync(projectId);
            if (ndaDoc == null || ndaDoc.Status != 2) // 2 = Signed
                return false;

            // Check if buyer has signed
            var buyerSignature = await _context.ESignSignatures
                .FirstOrDefaultAsync(s => s.DocumentId == ndaDoc.Id 
                    && s.SignerUserId == buyerUserId 
                    && s.Status == 1); // 1 = Signed

            return buyerSignature != null;
        }

        public async Task<ESignDocument?> GetProjectNdaAsync(int projectId)
        {
            return await _context.ESignDocuments
                .Include(d => d.Signatures)
                .Where(d => d.ProjectId == projectId && d.DocType == 1) // 1 = ProjectNDA
                .OrderByDescending(d => d.Id) // Get latest document
                .FirstOrDefaultAsync();
        }

        public async Task<ESignDocument> CreateDocumentAsync(int projectId, int docType, string documentName, int createdBy)
        {
            var doc = new ESignDocument
            {
                ProjectId = projectId,
                DocType = docType,
                DocumentName = documentName,
                Status = 0, // Draft
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.ESignDocuments.Add(doc);
            await _context.SaveChangesAsync();

            await LogActionAsync(doc.Id, createdBy, "CreateDocument", $"Created document: {documentName}");

            return doc;
        }

        public async Task<string> UploadDocumentAsync(long documentId, Stream fileStream, string fileName)
        {
            var doc = await _context.ESignDocuments.FindAsync(documentId);
            if (doc == null)
                throw new InvalidOperationException("Document not found");

            // Create uploads directory if not exists
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "esign");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{documentId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file and calculate hash
            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOut);
            }

            // Calculate SHA256 hash
            var hash = await CalculateFileHashAsync(filePath);

            // Update document
            doc.FilePath = $"/uploads/esign/{uniqueFileName}";
            doc.FileHash = hash;
            doc.Status = 1; // Pending signature

            await _context.SaveChangesAsync();

            await LogActionAsync(documentId, doc.CreatedBy ?? 0, "UploadDocument", $"Uploaded file: {fileName}, Hash: {hash}");

            return hash;
        }

        public async Task<string?> SendOtpAsync(int userId, string phoneNumber)
        {
            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // In production, send SMS via SMS gateway (VNPT, Viettel, Twilio)
            // For now, just log it
            _logger.LogInformation($"OTP for user {userId} ({phoneNumber}): {otp}");

            // TODO: Integrate with SMS gateway
            // await _smsService.SendAsync(phoneNumber, $"Your OTP code: {otp}");

            // For testing, return OTP (remove in production)
            var isTestMode = _configuration.GetValue<bool>("ESign:TestMode", true);
            return isTestMode ? otp : null;
        }

        public async Task<bool> VerifyOtpAsync(long signatureId, string otpCode)
        {
            var signature = await _context.ESignSignatures.FindAsync(signatureId);
            if (signature == null)
                return false;

            // In production, verify against stored hash
            // For now, simple comparison (testing mode)
            var isTestMode = _configuration.GetValue<bool>("ESign:TestMode", true);
            if (isTestMode)
            {
                // In test mode, accept any 6-digit code
                if (otpCode.Length == 6 && int.TryParse(otpCode, out _))
                {
                    signature.OtpVerifiedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            else
            {
                // Production: verify against hashed OTP
                var otpHash = HashString(otpCode);
                if (signature.OtpCodeHash == otpHash)
                {
                    signature.OtpVerifiedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task SignDocumentAsync(long documentId, int userId, string signerRole, string? ipAddress = null, string? userAgent = null)
        {
            var doc = await _context.ESignDocuments.FindAsync(documentId);
            if (doc == null)
                throw new InvalidOperationException("Document not found");

            // Check if signature already exists
            var existingSignature = await _context.ESignSignatures
                .FirstOrDefaultAsync(s => s.DocumentId == documentId && s.SignerUserId == userId);

            if (existingSignature != null)
            {
                // Update existing signature
                existingSignature.Status = 1; // Signed
                existingSignature.SignedAt = DateTime.UtcNow;
                existingSignature.IpAddress = ipAddress;
                existingSignature.UserAgent = userAgent;
                existingSignature.SignatureHash = GenerateSignatureHash(documentId, userId);
            }
            else
            {
                // Create new signature
                var signature = new ESignSignature
                {
                    DocumentId = documentId,
                    SignerUserId = userId,
                    SignerRole = signerRole,
                    Status = 1, // Signed
                    SignedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    SignatureHash = GenerateSignatureHash(documentId, userId),
                    OtpVerifiedAt = DateTime.UtcNow
                };

                _context.ESignSignatures.Add(signature);
            }

            // Update document status to Signed
            doc.Status = 2; // Signed
            doc.SignedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogActionAsync(documentId, userId, "SignDocument", $"Document signed by user {userId}", ipAddress, userAgent);
        }

        public async Task<List<ESignSignature>> GetDocumentSignaturesAsync(long documentId)
        {
            return await _context.ESignSignatures
                .Where(s => s.DocumentId == documentId)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task LogActionAsync(long documentId, int userId, string action, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var log = new ESignAuditLog
            {
                DocumentId = documentId,
                UserId = userId,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            _context.ESignAuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserSignedProjectNda(int projectId, int userId)
        {
            var ndaDoc = await GetProjectNdaAsync(projectId);
            if (ndaDoc == null || ndaDoc.Status != 2) // 2 = Signed
                return false;

            // Check if the specific user has signed
            var userSignature = await _context.ESignSignatures
                .FirstOrDefaultAsync(s => s.DocumentId == ndaDoc.Id
                    && s.SignerUserId == userId
                    && s.Status == 1); // 1 = Signed

            return userSignature != null;
        }

        // Helper methods
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private string GenerateSignatureHash(long documentId, int userId)
        {
            var data = $"{documentId}:{userId}:{DateTime.UtcNow:O}";
            return HashString(data);
        }
    }
}
