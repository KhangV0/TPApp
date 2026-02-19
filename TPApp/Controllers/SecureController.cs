using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class SecureController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        public SecureController(IConfiguration config, IEmailSender emailSender, ISmsSender smsSender)
        {
            _config      = config;
            _emailSender = emailSender;
            _smsSender   = smsSender;
        }

        public IActionResult Index()
        {
            return Content($"Hello {User.Identity?.Name}, this is a secure area!");
        }

        /// <summary>
        /// Admin tool: encrypt a plain-text value using EncryptKeyword from appsettings.
        /// Usage: GET /Secure/Encrypt?value=yourpassword&field=SMTP_PASSWORD
        /// </summary>
        [HttpGet]
        public IActionResult Encrypt(string value = "", string field = "SMTP_PASSWORD")
        {
            if (string.IsNullOrEmpty(value))
                return Content("Usage: /Secure/Encrypt?value=yourplaintext&field=FIELD_NAME\n\nAvailable sensitive fields:\n  SMTP_PASSWORD\n  SMS_AUTH_TOKEN\n  SMS_ACCOUNT_ID");

            var encryptKeyword = _config["AppSettings:EncryptKeyword"];
            if (string.IsNullOrEmpty(encryptKeyword))
                return Content("ERROR: AppSettings:EncryptKeyword not configured in appsettings.json");

            var textBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var keyBytes  = System.Text.Encoding.UTF8.GetBytes(encryptKeyword);

            for (int i = 0; i < textBytes.Length; i++)
                textBytes[i] ^= keyBytes[i % keyBytes.Length];

            var encrypted = Convert.ToBase64String(textBytes);

            return Content(
                $"Field:     {field}\n" +
                $"Encrypted: {encrypted}\n\n" +
                $"SQL:\nUPDATE SYS_PARAMETERS SET Val = '{encrypted}' WHERE Name = '{field}';\n\n" +
                $"-- Or INSERT if not exists:\n" +
                $"IF NOT EXISTS (SELECT 1 FROM SYS_PARAMETERS WHERE Name = '{field}')\n" +
                $"  INSERT INTO SYS_PARAMETERS (Name, Val, Activated, Domain) VALUES ('{field}', '{encrypted}', 1, 'techport');\n" +
                $"ELSE\n" +
                $"  UPDATE SYS_PARAMETERS SET Val = '{encrypted}' WHERE Name = '{field}';",
                "text/plain");
        }

        /// <summary>
        /// Admin tool: decrypt a Base64 encrypted value.
        /// Usage: GET /Secure/Decrypt?value=encryptedBase64
        /// </summary>
        [HttpGet]
        public IActionResult Decrypt(string value = "")
        {
            if (string.IsNullOrEmpty(value))
                return Content("Usage: /Secure/Decrypt?value=encryptedBase64");

            var encryptKeyword = _config["AppSettings:EncryptKeyword"];
            if (string.IsNullOrEmpty(encryptKeyword))
                return Content("ERROR: AppSettings:EncryptKeyword not configured.");

            try
            {
                var bytes    = Convert.FromBase64String(value);
                var keyBytes = System.Text.Encoding.UTF8.GetBytes(encryptKeyword);
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] ^= keyBytes[i % keyBytes.Length];

                return Content($"Decrypted: {System.Text.Encoding.UTF8.GetString(bytes)}", "text/plain");
            }
            catch
            {
                return Content("ERROR: Invalid Base64 or wrong EncryptKeyword.");
            }
        }

        /// <summary>
        /// Admin tool: send a test email to verify SMTP config in SYS_PARAMETERS.
        /// Usage: GET /Secure/TestEmail?to=your@email.com
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TestEmail(string to = "")
        {
            if (string.IsNullOrEmpty(to))
                return Content("Usage: /Secure/TestEmail?to=recipient@email.com");

            var subject = "[TechPort] Test Email - SMTP Config OK";
            var body    = $@"
<div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden'>
  <div style='background:#1a73e8;padding:20px;text-align:center'>
    <h2 style='color:white;margin:0'>TechPort – Test Email</h2>
  </div>
  <div style='padding:24px'>
    <p>✅ SMTP configuration is working correctly.</p>
    <p><strong>Sent at:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
    <p><strong>Sent to:</strong> {to}</p>
    <p><strong>Sent by:</strong> {User.Identity?.Name}</p>
    <hr style='border:none;border-top:1px solid #e0e0e0;margin:20px 0'/>
    <p style='color:#888;font-size:12px'>This is an automated test email from TechPort admin tools.</p>
  </div>
</div>";

            try
            {
                await _emailSender.SendAsync(to, subject, body, isHtml: true);
                return Content($"✅ Test email sent successfully to: {to}\nTime: {DateTime.Now:HH:mm:ss}", "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"❌ Failed to send email to: {to}\n\nError: {ex.Message}\n\nCheck SYS_PARAMETERS: SMTP_HOST, SMTP_PORT, SMTP_EMAIL, SMTP_PASSWORD", "text/plain");
            }
        }

        /// <summary>
        /// Admin tool: send a test SMS to verify SMS config in SYS_PARAMETERS.
        /// Usage: GET /Secure/TestSms?to=+84xxxxxxxxx
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TestSms(string to = "")
        {
            if (string.IsNullOrEmpty(to))
                return Content("Usage: /Secure/TestSms?to=+84xxxxxxxxx\n\nPhone number must include country code (e.g. +84912345678)");

            try
            {
                await _smsSender.SendAsync(to, $"[TechPort] Test SMS - Config OK. Sent at {DateTime.Now:HH:mm:ss}");
                return Content($"✅ Test SMS sent successfully to: {to}\nTime: {DateTime.Now:HH:mm:ss}", "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"❌ Failed to send SMS to: {to}\n\nError: {ex.Message}\n\nCheck SYS_PARAMETERS: SMS_ACCOUNT_ID, SMS_AUTH_TOKEN, SMS_FROM_NUMBER", "text/plain");
            }
        }
    }
}
