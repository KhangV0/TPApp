using Microsoft.Extensions.Options;
using TPApp.Configuration;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class OtpEmailService : IOtpEmailService
    {
        private readonly ISystemParameterService _params;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<OtpEmailService> _logger;
        private readonly OtpSettings _otp;

        public OtpEmailService(
            ISystemParameterService @params,
            IEmailSender emailSender,
            ILogger<OtpEmailService> logger,
            IOptions<OtpSettings> otpSettings)
        {
            _params      = @params;
            _emailSender = emailSender;
            _logger      = logger;
            _otp         = otpSettings.Value;
        }

        public async Task SendOtpAsync(string toEmail, string fullName, string otp, string role, int projectId)
        {
            // MockMode: read from SYS_PARAMETERS (NOTIFICATION_ENABLE_EMAIL = 0 means mock/disabled)
            bool enableEmail = await _params.GetBoolAsync(ParameterKeys.NotificationEnableEmail, true);

            if (!enableEmail)
            {
                _logger.LogInformation(
                    "=== [OTP MOCK] ===\n  To: {Email}\n  Name: {Name}\n  Role: {Role}\n  Project: {ProjectId}\n  OTP: {Otp}\n  Expires: {Expire}",
                    toEmail, fullName, role, projectId, otp, DateTime.Now.Add(_otp.NegotiationOtpExpiry).ToString("HH:mm:ss"));
                return;
            }

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
    <p style='color:#d32f2f'><strong>⚠ Mã này có hiệu lực trong {_otp.NegotiationOtpExpirySeconds / 60} phút.</strong> Không chia sẻ mã này với bất kỳ ai.</p>
    <hr style='border:none;border-top:1px solid #e0e0e0;margin:20px 0'/>
    <p style='color:#888;font-size:12px'>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
  </div>
</div>";

            await _emailSender.SendAsync(toEmail, subject, body, isHtml: true);
        }
    }
}
