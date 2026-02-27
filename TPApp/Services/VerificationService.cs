using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPApp.Configuration;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly AppDbContext      _context;
        private readonly IEmailSender      _email;
        private readonly ISmsSender        _sms;
        private readonly ILogger<VerificationService> _logger;
        private readonly OtpSettings       _otp;

        public VerificationService(
            AppDbContext context,
            IEmailSender email,
            ISmsSender sms,
            ILogger<VerificationService> logger,
            IOptions<OtpSettings> otpSettings)
        {
            _context = context;
            _email   = email;
            _sms     = sms;
            _logger  = logger;
            _otp     = otpSettings.Value;
        }

        // ─── Send Email OTP ──────────────────────────────────────────────────
        public async Task<bool> SendEmailOtpAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            var otp = GenerateOtp();
            await SaveOtpAsync(userId, 1, otp);

            var subject = "[TechPort] Xác thực địa chỉ Email của bạn";
            var body = BuildOtpEmailBody(user.FullName ?? user.UserName!, otp, "Email");
            await _email.SendAsync(user.Email, subject, body, isHtml: true);

            _logger.LogInformation("[VerifyEmail] OTP {Otp} sent to {Email}", otp, user.Email);
            return true;
        }

        // ─── Send Phone OTP ──────────────────────────────────────────────────
        public async Task<bool> SendPhoneOtpAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PhoneNumber)) return false;

            var otp = GenerateOtp();
            await SaveOtpAsync(userId, 2, otp);

            await _sms.SendAsync(user.PhoneNumber, $"[TechPort] Mã OTP xác thực SĐT của bạn: {otp}. Có hiệu lực {_otp.PhoneOtpExpiryMinutes} phút.");
            _logger.LogInformation("[VerifyPhone] OTP {Otp} sent to {Phone}", otp, user.PhoneNumber);
            return true;
        }

        // ─── Verify Email OTP ────────────────────────────────────────────────
        public async Task<(bool ok, string msg)> VerifyEmailOtpAsync(int userId, string otp)
        {
            var result = await ConsumeOtpAsync(userId, 1, otp);
            if (!result) return (false, "Mã OTP không đúng hoặc đã hết hạn.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            user.EmailVerified = true;
            UpdateVerificationLevel(user);
            await _context.SaveChangesAsync();
            return (true, "✅ Email đã được xác thực thành công!");
        }

        // ─── Verify Phone OTP ────────────────────────────────────────────────
        public async Task<(bool ok, string msg)> VerifyPhoneOtpAsync(int userId, string otp)
        {
            var result = await ConsumeOtpAsync(userId, 2, otp);
            if (!result) return (false, "Mã OTP không đúng hoặc đã hết hạn.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            user.PhoneVerified = true;
            UpdateVerificationLevel(user);
            await _context.SaveChangesAsync();
            return (true, "✅ Số điện thoại đã được xác thực thành công!");
        }

        // ─── Upload Doc ──────────────────────────────────────────────────────
        public async Task<(bool ok, string msg)> UploadDocAsync(int userId, int docType, IFormFile file, IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0)
                return (false, "Vui lòng chọn file hợp lệ.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            if (!allowed.Contains(ext))
                return (false, "Chỉ chấp nhận JPG, PNG hoặc PDF.");
            if (file.Length > 5 * 1024 * 1024)
                return (false, "File không được vượt quá 5MB.");

            var folder = Path.Combine(env.WebRootPath, "uploads", "verify", userId.ToString());
            Directory.CreateDirectory(folder);

            var safeName = $"doc_{docType}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var fullPath = Path.Combine(folder, safeName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            // Remove old doc of same type for this user
            var old = await _context.UserVerificationDocs
                .Where(d => d.UserId == userId && d.DocType == docType)
                .ToListAsync();
            _context.UserVerificationDocs.RemoveRange(old);

            _context.UserVerificationDocs.Add(new UserVerificationDoc
            {
                UserId     = userId,
                DocType    = docType,
                FilePath   = $"/uploads/verify/{userId}/{safeName}",
                FileName   = file.FileName,
                FileSize   = file.Length,
                UploadedAt = DateTime.UtcNow,
                ReviewStatus = 0
            });

            // Update verification level if docs are complete
            var user = await _context.Users.FindAsync(userId);
            if (user != null) UpdateVerificationLevel(user);

            await _context.SaveChangesAsync();

            var label = docType == DocType.CccdFront ? "CCCD mặt trước"
                      : docType == DocType.CccdBack  ? "CCCD mặt sau"
                      : "Giấy phép kinh doanh";
            return (true, $"✅ {label} đã được tải lên. Đang chờ xem xét.");
        }

        // ─── Get Docs ────────────────────────────────────────────────────────
        public async Task<List<VerifyDocVm>> GetDocsAsync(int userId)
        {
            return await _context.UserVerificationDocs
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.DocType)
                .Select(d => new VerifyDocVm
                {
                    Id           = d.Id,
                    DocType      = d.DocType,
                    FileName     = d.FileName,
                    FilePath     = d.FilePath,
                    UploadedAt   = d.UploadedAt,
                    ReviewStatus = d.ReviewStatus
                })
                .ToListAsync();
        }

        // ─── Update Phone ────────────────────────────────────────────────────
        public async Task<bool> UpdatePhoneAsync(int userId, string phone)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Phone changed → reset phone verification
            if (user.PhoneNumber != phone)
            {
                user.PhoneVerified = false;
                UpdateVerificationLevel(user);
            }
            user.PhoneNumber = phone;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Private Helpers ─────────────────────────────────────────────────

        private static string GenerateOtp() =>
            new Random().Next(100000, 999999).ToString();

        private async Task SaveOtpAsync(int userId, int otpType, string code)
        {
            // Invalidate old unused OTPs of same type
            var old = await _context.UserOtps
                .Where(o => o.UserId == userId && o.OtpType == otpType && !o.IsUsed)
                .ToListAsync();
            old.ForEach(o => o.IsUsed = true);

            _context.UserOtps.Add(new UserOtp
            {
                UserId    = userId,
                OtpType   = otpType,
                OtpCode   = code,
                ExpiresAt = DateTime.UtcNow.Add(otpType == 1 ? _otp.EmailOtpExpiry : _otp.PhoneOtpExpiry),
                IsUsed    = false,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task<bool> ConsumeOtpAsync(int userId, int otpType, string code)
        {
            var otp = await _context.UserOtps
                .Where(o => o.UserId == userId && o.OtpType == otpType
                         && o.OtpCode == code && !o.IsUsed
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null) return false;
            otp.IsUsed = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private static void UpdateVerificationLevel(Entities.ApplicationUser user)
        {
            // Determine required docs based on account type
            var hasAllDocs = false; // will be computed separately if needed (not async here)
            if (user.PhoneVerified && user.EmailVerified)
                user.VerificationLevel = 3;
            else if (user.EmailVerified)
                user.VerificationLevel = 2;
            else if (user.PhoneVerified)
                user.VerificationLevel = 1;
            else
                user.VerificationLevel = 0;
        }

    private string BuildOtpEmailBody(string name, string otp, string channel)
    {
        int minutes = channel == "Email" ? _otp.EmailOtpExpiryMinutes : _otp.PhoneOtpExpiryMinutes;
        return $@"
<div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden'>
  <div style='background:#1a73e8;padding:20px;text-align:center'>
    <h2 style='color:white;margin:0'>TechPort – Xác thực tài khoản</h2>
  </div>
  <div style='padding:24px'>
    <p>Xin chào <strong>{name}</strong>,</p>
    <p>Đây là mã OTP để xác thực <strong>{channel}</strong> của bạn:</p>
    <div style='text-align:center;margin:24px 0'>
      <span style='font-size:36px;font-weight:bold;letter-spacing:8px;color:#1a73e8;background:#f0f4ff;padding:12px 24px;border-radius:8px'>{otp}</span>
    </div>
    <p style='color:#d32f2f'><strong>⚠ Mã có hiệu lực {minutes} phút.</strong> Không chia sẻ mã này với bất kỳ ai.</p>
  </div>
</div>";
    }
    }
}
