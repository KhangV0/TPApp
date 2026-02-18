using System.Net;
using System.Net.Mail;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class OtpEmailService : IOtpEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OtpEmailService> _logger;

        public OtpEmailService(IConfiguration config, ILogger<OtpEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendOtpAsync(string toEmail, string fullName, string otp, string role, int projectId)
        {
            bool mockMode = _config.GetValue<bool>("Email:MockMode", true);

            if (mockMode)
            {
                // DEV MODE: log OTP to console instead of sending email
                _logger.LogInformation(
                    "=== [OTP MOCK] ===\n  To: {Email}\n  Name: {Name}\n  Role: {Role}\n  Project: {ProjectId}\n  OTP: {Otp}\n  Expires: {Expire}",
                    toEmail, fullName, role, projectId, otp, DateTime.Now.AddMinutes(5).ToString("HH:mm:ss"));
                return;
            }

            // PRODUCTION: send real email
            var host     = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port     = _config.GetValue<int>("Email:SmtpPort", 587);
            var username = _config["Email:Username"] ?? "";
            var password = _config["Email:Password"] ?? "";
            var from     = _config["Email:From"] ?? "noreply@techport.vn";

            var subject = $"[TechPort] Mã OTP ký biên bản thương lượng - Dự án #{projectId}";
            var body = $@"
<div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden'>
  <div style='background:#1a73e8;padding:20px;text-align:center'>
    <h2 style='color:white;margin:0'>TechPort – Ký số biên bản</h2>
  </div>
  <div style='padding:24px'>
    <p>Xin chào <strong>{fullName}</strong>,</p>
    <p>Bạn đang ký biên bản thương lượng với vai trò <strong>{role}</strong> cho <strong>Dự án #{projectId}</strong>.</p>
    <p>Mã OTP của bạn:</p>
    <div style='text-align:center;margin:24px 0'>
      <span style='font-size:36px;font-weight:bold;letter-spacing:8px;color:#1a73e8;background:#f0f4ff;padding:12px 24px;border-radius:8px'>{otp}</span>
    </div>
    <p style='color:#d32f2f'><strong>⚠ Mã này có hiệu lực trong 5 phút.</strong> Không chia sẻ mã này với bất kỳ ai.</p>
    <hr style='border:none;border-top:1px solid #e0e0e0;margin:20px 0'/>
    <p style='color:#888;font-size:12px'>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
  </div>
</div>";

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mail = new MailMessage(from, toEmail, subject, body) { IsBodyHtml = true };
            await client.SendMailAsync(mail);
        }
    }
}
