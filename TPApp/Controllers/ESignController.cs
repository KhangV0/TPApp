using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    public class ESignController : Controller
    {
        private readonly IESignGateway _eSignGateway;
        private readonly ILogger<ESignController> _logger;

        public ESignController(IESignGateway eSignGateway, ILogger<ESignController> logger)
        {
            _eSignGateway = eSignGateway;
            _logger = logger;
        }

        [HttpPost]
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

                // Get current user ID
                var userId = GetCurrentUserId();

                // 1. Create NDAAgreement record first
                var ndaAgreement = new TPApp.Entities.NDAAgreement
                {
                    ProjectId = projectId,
                    BenA = benA,
                    BenB = benB,
                    LoaiNDA = loaiNDA,
                    ThoiHanBaoMat = thoiHanBaoMat,
                    DaDongY = daDongY,
                    XacNhanKySo = null, // Will be updated after E-Sign
                    StatusId = 1, // Active
                    NguoiTao = userId,
                    NgayTao = DateTime.Now
                };

                // Save to database (need DbContext)
                var dbContext = HttpContext.RequestServices.GetRequiredService<TPApp.Data.AppDbContext>();
                dbContext.NDAAgreements.Add(ndaAgreement);
                await dbContext.SaveChangesAsync();

                // 2. Create E-Sign document
                var doc = await _eSignGateway.CreateDocumentAsync(
                    projectId, 
                    1, // DocType = 1 (ProjectNDA)
                    $"NDA - {benA} & {benB}", 
                    userId
                );

                // 3. Upload file
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
        public async Task<IActionResult> SendOtp(string phoneNumber)
        {
            try
            {
                var userId = GetCurrentUserId();
                var otp = await _eSignGateway.SendOtpAsync(userId, phoneNumber);

                return Json(new { 
                    success = true, 
                    otp = otp, // Only in test mode
                    message = "OTP đã được gửi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignDocument(long documentId, string otpCode)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // For now, skip OTP verification in test mode
                // In production, verify OTP first

                // Get IP and User Agent
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // Sign document
                await _eSignGateway.SignDocumentAsync(
                    documentId, 
                    userId, 
                    "Buyer", // Role
                    ipAddress, 
                    userAgent
                );

                // Get document to find project ID
                var dbContext = HttpContext.RequestServices.GetRequiredService<TPApp.Data.AppDbContext>();
                var doc = await dbContext.ESignDocuments.FindAsync(documentId);
                
                if (doc != null && doc.ProjectId > 0)
                {
                    var projectId = doc.ProjectId;

                    // 1. Update NDAAgreement - mark as signed
                    var ndaAgreement = await dbContext.NDAAgreements
                        .FirstOrDefaultAsync(n => n.ProjectId == projectId);
                    
                    if (ndaAgreement != null)
                    {
                        ndaAgreement.XacNhanKySo = "Đã ký điện tử";
                        ndaAgreement.DaDongY = true;
                        ndaAgreement.NgaySua = DateTime.Now;
                        ndaAgreement.NguoiSua = userId;
                    }

                    // 2. Update ProjectSteps - Mark Step 2 as Completed (StatusId = 2)
                    var step2 = await dbContext.ProjectSteps
                        .FirstOrDefaultAsync(ps => ps.ProjectId == projectId && ps.StepNumber == 2);
                    
                    if (step2 != null)
                    {
                        step2.StatusId = 2; // Completed
                        step2.CompletedDate = DateTime.Now;
                    }

                    // 3. Unlock Step 3 (set StatusId = 1 if it's 0)
                    var step3 = await dbContext.ProjectSteps
                        .FirstOrDefaultAsync(ps => ps.ProjectId == projectId && ps.StepNumber == 3);
                    
                    if (step3 != null && step3.StatusId == 0)
                    {
                        step3.StatusId = 1; // Active/In Progress
                    }

                    await dbContext.SaveChangesAsync();
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
            // TODO: Get from authentication
            // For now, return a test user ID
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            
            // Fallback for testing
            return 1;
        }
    }
}
