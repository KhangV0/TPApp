using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TPApp.Controllers
{
    [Authorize]
    public class NegotiationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly Services.IWorkflowService _workflowService;
        private readonly IOtpEmailService _otpEmailService;

        public NegotiationController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            Services.IWorkflowService workflowService,
            IOtpEmailService otpEmailService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _workflowService = workflowService;
            _otpEmailService = otpEmailService;
        }

        private int GetCurrentUserId()
        {
            var s = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(s) || !int.TryParse(s, out int id))
                throw new UnauthorizedAccessException("Invalid user ID");
            return id;
        }

        /// <summary>Returns (canAccess, isBuyer, isSeller) for current user on this project.</summary>
        private async Task<(bool canAccess, bool isBuyer, bool isSeller)> GetAccessAsync(int projectId, int userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return (false, false, false);
            bool isBuyer  = project.CreatedBy == userId;
            bool isSeller = project.SelectedSellerId == userId;
            return (isBuyer || isSeller, isBuyer, isSeller);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /Negotiation/Edit?projectId=5
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int? projectId)
        {
            if (projectId == null) return NotFound();
            var userId = GetCurrentUserId();
            var (canAccess, _, _) = await GetAccessAsync(projectId.Value, userId);
            if (!canAccess) return Forbid();
            if (!await _workflowService.CanAccessStep(projectId.Value, 5)) return Forbid();

            var negotiation = await _context.NegotiationForms
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (negotiation == null) return NotFound("Chưa có biên bản thương lượng.");
            if (negotiation.StatusId == (int)NegotiationStatus.Completed)
                return BadRequest("Bước thương lượng đã hoàn tất, không thể chỉnh sửa.");

            return View(negotiation);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /Negotiation/Edit
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NegotiationForm model, IFormFile? BienBanFile)
        {
            var userId = GetCurrentUserId();
            var (canAccess, _, _) = await GetAccessAsync(model.ProjectId ?? 0, userId);
            if (!canAccess) return Forbid();

            var negotiation = await _context.NegotiationForms
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.ProjectId == model.ProjectId);
            if (negotiation == null) return NotFound();
            if (negotiation.StatusId == (int)NegotiationStatus.Completed)
                return BadRequest("Bước thương lượng đã hoàn tất, không thể chỉnh sửa.");

            ModelState.Remove("BienBanThuongLuongFile");
            ModelState.Remove("SellerId");
            ModelState.Remove("DieuKhoanThanhToan");
            ModelState.Remove("HinhThucKy");

            // Handle file upload
            if (BienBanFile != null && BienBanFile.Length > 0)
            {
                var allowed = new[] { ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(BienBanFile.FileName).ToLower();
                if (!allowed.Contains(ext))
                { ModelState.AddModelError("BienBanThuongLuongFile", "Chỉ chấp nhận .pdf, .doc, .docx"); return View(model); }
                if (BienBanFile.Length > 20 * 1024 * 1024)
                { ModelState.AddModelError("BienBanThuongLuongFile", "File không quá 20MB."); return View(model); }

                string folder = Path.Combine(_environment.WebRootPath, "uploads", "negotiations");
                Directory.CreateDirectory(folder);
                string fname = $"{Guid.NewGuid()}_{BienBanFile.FileName}";
                using var stream = new FileStream(Path.Combine(folder, fname), FileMode.Create);
                await BienBanFile.CopyToAsync(stream);
                negotiation.BienBanThuongLuongFile = $"/uploads/negotiations/{fname}";
            }

            negotiation.GiaChotCuoiCung   = model.GiaChotCuoiCung;
            negotiation.DieuKhoanThanhToan = model.DieuKhoanThanhToan;
            negotiation.HinhThucKy         = model.HinhThucKy;
            negotiation.NguoiSua           = userId;
            negotiation.NgaySua            = DateTime.Now;

            if (model.HinhThucKy == "E-Sign" || model.HinhThucKy == "OTP")
                negotiation.DaKySo = true;

            // Advance to WaitingSignature when price is set
            if (negotiation.GiaChotCuoiCung.HasValue &&
                negotiation.StatusId < (int)NegotiationStatus.WaitingSignature)
            {
                negotiation.StatusId = (int)NegotiationStatus.WaitingSignature;
            }

            await _context.SaveChangesAsync();
            return Redirect($"/Project/Details/{model.ProjectId}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /Negotiation/RequestOtp  (AJAX)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, isBuyer, isSeller) = await GetAccessAsync(dto.ProjectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });

                var negotiation = await _context.NegotiationForms
                    .FirstOrDefaultAsync(x => x.ProjectId == dto.ProjectId);
                if (negotiation == null)
                    return Json(new { success = false, message = "Chưa có biên bản thương lượng." });

                if (negotiation.StatusId == (int)NegotiationStatus.Completed)
                    return Json(new { success = false, message = "Bước thương lượng đã hoàn tất." });

                if (negotiation.StatusId < (int)NegotiationStatus.WaitingSignature)
                    return Json(new { success = false, message = "Cần thống nhất giá trước khi ký." });

                // Guard: already signed
                if (isSeller && negotiation.SellerSigned)
                    return Json(new { success = false, message = "Seller đã ký rồi." });
                if (isBuyer && negotiation.BuyerSigned)
                    return Json(new { success = false, message = "Buyer đã ký rồi." });

                // Generate 6-digit OTP
                var otp = new Random().Next(100000, 999999).ToString();
                var expire = DateTime.Now.AddMinutes(5);

                if (isSeller) { negotiation.SellerOtpCode = otp; negotiation.SellerOtpExpire = expire; }
                if (isBuyer)  { negotiation.BuyerOtpCode  = otp; negotiation.BuyerOtpExpire  = expire; }
                await _context.SaveChangesAsync();

                // Get user email
                var user = await _context.Users.FindAsync(userId);
                var email    = user?.Email ?? "";
                var fullName = user?.FullName ?? user?.UserName ?? "Người dùng";
                var role     = isBuyer ? "Buyer" : "Seller";

                await _otpEmailService.SendOtpAsync(email, fullName, otp, role, dto.ProjectId);

                return Json(new { success = true, message = $"OTP đã gửi đến {email}. Có hiệu lực 5 phút." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /Negotiation/VerifyOtp  (AJAX)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, isBuyer, isSeller) = await GetAccessAsync(dto.ProjectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });

                var negotiation = await _context.NegotiationForms
                    .FirstOrDefaultAsync(x => x.ProjectId == dto.ProjectId);
                if (negotiation == null)
                    return Json(new { success = false, message = "Không tìm thấy biên bản." });

                if (negotiation.StatusId == (int)NegotiationStatus.Completed)
                    return Json(new { success = false, message = "Bước thương lượng đã hoàn tất." });

                // Validate OTP
                string? storedOtp    = isSeller ? negotiation.SellerOtpCode   : negotiation.BuyerOtpCode;
                DateTime? storedExp  = isSeller ? negotiation.SellerOtpExpire : negotiation.BuyerOtpExpire;

                if (string.IsNullOrEmpty(storedOtp))
                    return Json(new { success = false, message = "Chưa có OTP. Vui lòng yêu cầu OTP trước." });

                if (DateTime.Now > storedExp)
                {
                    // Clear expired OTP
                    if (isSeller) { negotiation.SellerOtpCode = null; negotiation.SellerOtpExpire = null; }
                    if (isBuyer)  { negotiation.BuyerOtpCode  = null; negotiation.BuyerOtpExpire  = null; }
                    await _context.SaveChangesAsync();
                    return Json(new { success = false, message = "OTP đã hết hạn. Vui lòng yêu cầu OTP mới." });
                }

                if (storedOtp != dto.Otp.Trim())
                    return Json(new { success = false, message = "OTP không đúng. Vui lòng kiểm tra lại." });

                // OTP valid → sign
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;
                    if (isSeller)
                    {
                        negotiation.SellerSigned    = true;
                        negotiation.SellerSignedAt  = now;
                        negotiation.SellerOtpCode   = null;
                        negotiation.SellerOtpExpire = null;
                    }
                    if (isBuyer)
                    {
                        negotiation.BuyerSigned    = true;
                        negotiation.BuyerSignedAt  = now;
                        negotiation.BuyerOtpCode   = null;
                        negotiation.BuyerOtpExpire = null;
                    }
                    negotiation.NguoiSua = userId;
                    negotiation.NgaySua  = now;

                    bool bothSigned = negotiation.SellerSigned && negotiation.BuyerSigned;
                    if (bothSigned)
                    {
                        negotiation.StatusId = (int)NegotiationStatus.Completed;
                        await _context.SaveChangesAsync();
                        await _workflowService.CompleteStep(dto.ProjectId, 5);
                    }
                    else
                    {
                        negotiation.StatusId = (int)NegotiationStatus.PartiallySigned;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    string msg = bothSigned
                        ? "Cả hai bên đã ký! Bước 5 hoàn tất. Bước 6 đã được mở."
                        : (isSeller ? "Seller đã ký." : "Buyer đã ký.") + " Đang chờ bên còn lại ký.";

                    return Json(new { success = true, completed = bothSigned, message = msg });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Lỗi khi ký: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Legacy redirect
        [HttpGet]
        public IActionResult Create(int? projectId) => RedirectToAction("Edit", new { projectId });

        [HttpGet]
        public IActionResult Success() => View();
    }

    // ─── DTOs ───────────────────────────────────────────────────────────────
    public class RequestOtpDto { public int ProjectId { get; set; } }
    public class VerifyOtpDto  { public int ProjectId { get; set; } public string Otp { get; set; } = ""; }
}
