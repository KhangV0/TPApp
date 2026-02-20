using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Interfaces;
using TPApp.Data;

namespace TPApp.Controllers
{
    public class ESignController : Controller
    {
        private readonly IESignGateway _eSignGateway;
        private readonly ILogger<ESignController> _logger;
        private readonly AppDbContext _context;
        private readonly IOtpEmailService _otpEmailService;
        private readonly ISmsSender _smsSender;

        public ESignController(
            IESignGateway eSignGateway,
            ILogger<ESignController> logger,
            AppDbContext context,
            IOtpEmailService otpEmailService,
            ISmsSender smsSender)
        {
            _eSignGateway = eSignGateway;
            _logger = logger;
            _context = context;
            _otpEmailService = otpEmailService;
            _smsSender = smsSender;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadNda(
            IFormFile ndaFile, 
            int projectId,
            string benA,
            string benB,
            string loaiNDA,
            string thoiHanBaoMat,
            bool daDongY)
        {
            try
            {
                if (ndaFile == null || ndaFile.Length == 0)
                    return Json(new { success = false, message = "File không hợp lệ" });

                var userId = GetCurrentUserId();

                var ndaAgreement = new TPApp.Entities.NDAAgreement
                {
                    ProjectId = projectId,
                    BenA = benA,
                    BenB = benB,
                    LoaiNDA = loaiNDA,
                    ThoiHanBaoMat = thoiHanBaoMat,
                    DaDongY = daDongY,
                    XacNhanKySo = null,
                    StatusId = 1,
                    NguoiTao = userId,
                    NgayTao = DateTime.Now
                };

                _context.NDAAgreements.Add(ndaAgreement);
                await _context.SaveChangesAsync();

                var doc = await _eSignGateway.CreateDocumentAsync(
                    projectId, 1, $"NDA - {benA} & {benB}", userId);

                using var stream = ndaFile.OpenReadStream();
                var hash = await _eSignGateway.UploadDocumentAsync(doc.Id, stream, ndaFile.FileName);

                return Json(new { 
                    success = true, 
                    documentId = doc.Id,
                    ndaId = ndaAgreement.Id,
                    hash = hash,
                    message = "Tạo Phiếu NDA và tải lên file thành công!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating NDA and uploading document");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendOtp([FromBody] ESignOtpRequestDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                var email = user?.Email ?? "";
                var phone = user?.PhoneNumber ?? dto?.PhoneNumber ?? "";
                var fullName = user?.FullName ?? user?.UserName ?? "Người dùng";
                var channel = dto?.Channel?.ToLower() ?? "email";

                // Generate 6-digit OTP and store via gateway
                var otp = await _eSignGateway.SendOtpAsync(userId, phone.Length > 0 ? phone : "email");

                if (channel == "sms")
                {
                    if (string.IsNullOrEmpty(phone))
                        return Json(new { success = false, message = "Không tìm thấy số điện thoại. Vui lòng nhập." });

                    var smsMessage = $"[Techport] Mã OTP ký NDA (Dự án #{dto?.ProjectId}): {otp}. Có hiệu lực 5 phút.";
                    await _smsSender.SendAsync(phone, smsMessage);
                    var maskedPhone = phone.Length > 4 ? new string('*', phone.Length - 4) + phone[^4..] : phone;
                    return Json(new { success = true, channel = "sms",
                        message = $"OTP đã gửi đến SMS {maskedPhone}. Có hiệu lực 5 phút." });
                }
                else
                {
                    if (string.IsNullOrEmpty(email))
                        return Json(new { success = false, message = "Không tìm thấy email. Vui lòng liên hệ quản trị viên." });

                    await _otpEmailService.SendOtpAsync(email, fullName, otp, "NDA Signer", dto?.ProjectId ?? 0);
                    var maskedEmail = email.Length > 3 ? email[..3] + "***" + email[email.IndexOf('@')..] : email;
                    return Json(new { success = true, channel = "email",
                        message = $"OTP đã gửi đến {maskedEmail}. Có hiệu lực 5 phút." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SignDocument([FromBody] ESignSignDocDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                await _eSignGateway.SignDocumentAsync(
                    dto.DocumentId, userId, "Buyer", ipAddress, userAgent);

                var doc = await _context.ESignDocuments.FindAsync(dto.DocumentId);
                
                if (doc != null && doc.ProjectId > 0)
                {
                    var projectId = doc.ProjectId;

                    var ndaAgreement = await _context.NDAAgreements
                        .FirstOrDefaultAsync(n => n.ProjectId == projectId);
                    
                    if (ndaAgreement != null)
                    {
                        ndaAgreement.XacNhanKySo = "Đã ký điện tử";
                        ndaAgreement.DaDongY = true;
                        ndaAgreement.NgaySua = DateTime.Now;
                        ndaAgreement.NguoiSua = userId;
                    }

                    var step2 = await _context.ProjectSteps
                        .FirstOrDefaultAsync(ps => ps.ProjectId == projectId && ps.StepNumber == 2);
                    
                    if (step2 != null)
                    {
                        step2.StatusId = 2;
                        step2.CompletedDate = DateTime.Now;
                    }

                    var step3 = await _context.ProjectSteps
                        .FirstOrDefaultAsync(ps => ps.ProjectId == projectId && ps.StepNumber == 3);
                    
                    if (step3 != null && step3.StatusId == 0)
                    {
                        step3.StatusId = 1;
                    }

                    await _context.SaveChangesAsync();
                }

                return Json(new { 
                    success = true, 
                    message = "Ký thành công! Bước 2 đã hoàn thành."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing document");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return 1;
        }
    }

    public class ESignOtpRequestDto
    {
        public int ProjectId { get; set; }
        public string? Channel { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ESignSignDocDto
    {
        public long DocumentId { get; set; }
        public string OtpCode { get; set; } = "";
    }
}
