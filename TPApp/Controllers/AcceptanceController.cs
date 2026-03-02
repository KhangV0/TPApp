using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class AcceptanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;
        private readonly IOtpEmailService _otpEmailService;
        private readonly IConfiguration _configuration;
        private readonly Services.INotificationQueueService _notifQueue;

        public AcceptanceController(AppDbContext context, UserManager<ApplicationUser> userManager,
            Services.IWorkflowService workflowService, IOtpEmailService otpEmailService,
            IConfiguration configuration, Services.INotificationQueueService notifQueue)
        {
            _context = context;
            _userManager = userManager;
            _workflowService = workflowService;
            _otpEmailService = otpEmailService;
            _configuration = configuration;
            _notifQueue = notifQueue;
        }

        // Helper method to get current user ID as int
        private int GetCurrentUserId()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }

        // GET: /Acceptance/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 10)
            if (!await _workflowService.CanAccessStep(projectId.Value, 10)) return Forbid();

            var existing = await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            var model = new AcceptanceReport
            {
                ProjectId = projectId,
                NgayNghiemThu = DateTime.Now
            };
            return View(model);
        }

        // POST: /Acceptance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcceptanceReport model)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    model.TrangThaiKy = "Chưa ký";
                    model.NguoiTao = userId;
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.AcceptanceReports.Add(model);
                    await _context.SaveChangesAsync();

                    // Complete Step 10
                    await _workflowService.CompleteStep(model.ProjectId.Value, 10);

                    return RedirectToAction("Details", "Project", new { id = model.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Acceptance/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: SignAsBenA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenA(int id)
        {
            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null) return NotFound();

            if (string.IsNullOrEmpty(report.ChuKyBenA))
            {
                report.ChuKyBenA = User.Identity?.Name ?? "Admin";
                
                if (!string.IsNullOrEmpty(report.ChuKyBenB))
                {
                    report.TrangThaiKy = "Hoàn tất";
                    report.StatusId = 2;
                }
                else
                {
                    report.TrangThaiKy = "Đã ký 1 bên";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: SignAsBenB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenB(int id)
        {
            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null) return NotFound();

            if (string.IsNullOrEmpty(report.ChuKyBenB))
            {
                report.ChuKyBenB = User.Identity?.Name ?? "Khách hàng";

                if (!string.IsNullOrEmpty(report.ChuKyBenA))
                {
                    report.TrangThaiKy = "Hoàn tất";
                    report.StatusId = 2;
                }
                else
                {
                    report.TrangThaiKy = "Đã ký 1 bên";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Acceptance/SaveInline (AJAX from Step 13)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(
            int projectId, DateTime NgayNghiemThu,
            string? ThanhPhanThamGia, string? KetLuanNghiemThu,
            string? VanDeTonDong)
        {
            try
            {
                var userId = GetCurrentUserId();

                var entity = await _context.AcceptanceReports
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);

                bool isNew = entity == null;
                if (isNew)
                {
                    entity = new AcceptanceReport
                    {
                        ProjectId = projectId,
                        TrangThaiKy = "Chưa ký",
                        NguoiTao = userId,
                        NgayTao = DateTime.Now
                    };
                }

                entity.NgayNghiemThu = NgayNghiemThu;
                entity.ThanhPhanThamGia = ThanhPhanThamGia;
                entity.KetLuanNghiemThu = KetLuanNghiemThu;
                entity.VanDeTonDong = VanDeTonDong;
                entity.StatusId = 1;
                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;

                if (isNew)
                    _context.AcceptanceReports.Add(entity);

                await _context.SaveChangesAsync();

                if (entity.ProjectId.HasValue)
                    await _workflowService.CompleteStep(entity.ProjectId.Value, 13);

                return Json(new { success = true, message = isNew ? "✅ Đã lưu nghiệm thu." : "✅ Đã cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: /Acceptance/SendOtp (AJAX)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendOtp(int projectId, string side)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return Json(new { success = false, message = "Không tìm thấy user." });

                var entity = await _context.AcceptanceReports
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);
                if (entity == null) return Json(new { success = false, message = "Chưa có biên bản nghiệm thu." });

                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                var hash = HashOtp(otp);

                // Store hash in ChuKyBenA/B temporarily (prefix with OTP-)
                if (side == "benA")
                    entity.ChuKyBenA = $"OTP-{hash}";
                else
                    entity.ChuKyBenB = $"OTP-{hash}";

                await _context.SaveChangesAsync();

                // Send email
                var email = user.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    var fullName = user.FullName ?? user.UserName ?? "User";
                    var role = side == "benA" ? "Bên A" : "Bên B";
                    await _otpEmailService.SendOtpAsync(email, fullName, otp, role, projectId);
                }

                var masked = !string.IsNullOrEmpty(email)
                    ? email[..2] + "***" + email[email.IndexOf('@')..]
                    : "(không có email)";

                return Json(new { success = true, message = $"Đã gửi OTP tới {masked}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: /Acceptance/VerifyOtp (AJAX)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> VerifyOtp(int projectId, string side, string otpCode)
        {
            try
            {
                var entity = await _context.AcceptanceReports
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);
                if (entity == null) return Json(new { success = false, message = "Không tìm thấy biên bản." });

                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                var userName = user?.FullName ?? user?.UserName ?? "User";

                var isTestMode = _configuration.GetValue<bool>("ESign:TestMode");
                bool otpOk;

                if (isTestMode)
                {
                    otpOk = otpCode.Length == 6 && int.TryParse(otpCode, out _);
                }
                else
                {
                    var storedHash = side == "benA" ? entity.ChuKyBenA : entity.ChuKyBenB;
                    if (string.IsNullOrEmpty(storedHash) || !storedHash.StartsWith("OTP-"))
                        return Json(new { success = false, message = "Chưa gửi OTP hoặc OTP đã hết hạn." });

                    var expectedHash = storedHash[4..]; // Remove "OTP-" prefix
                    otpOk = HashOtp(otpCode) == expectedHash;
                }

                if (!otpOk)
                    return Json(new { success = false, message = "Mã OTP không đúng. Vui lòng thử lại." });

                // Sign
                if (side == "benA")
                    entity.ChuKyBenA = userName;
                else
                    entity.ChuKyBenB = userName;

                // Update signing status
                if (!string.IsNullOrEmpty(entity.ChuKyBenA) && !entity.ChuKyBenA.StartsWith("OTP-")
                    && !string.IsNullOrEmpty(entity.ChuKyBenB) && !entity.ChuKyBenB.StartsWith("OTP-"))
                {
                    entity.TrangThaiKy = "Hoàn tất";
                    entity.StatusId = 2;
                }
                else
                {
                    entity.TrangThaiKy = "Đã ký 1 bên";
                }

                entity.NguoiSua = userId;
                entity.NgaySua = DateTime.Now;
                await _context.SaveChangesAsync();

                // Notify project members
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    var sideLabel = side == "benA" ? "Bên A" : "Bên B";
                    var notifMsg = $"{sideLabel} ({userName}) đã ký nghiệm thu thành công.";
                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId, "📋 Bước 13: Ký nghiệm thu", notifMsg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId, "📋 Bước 13: Ký nghiệm thu", notifMsg);
                }

                return Json(new { success = true, message = $"✅ {userName} đã ký nghiệm thu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private static string HashOtp(string otp)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
