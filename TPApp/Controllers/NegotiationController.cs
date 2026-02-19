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
        private readonly ISmsSender _smsSender;
        private readonly Services.INotificationQueueService _notifQueue;
        private readonly ILegalReviewService _legalReviewService;

        public NegotiationController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            Services.IWorkflowService workflowService,
            IOtpEmailService otpEmailService,
            ISmsSender smsSender,
            Services.INotificationQueueService notifQueue,
            ILegalReviewService legalReviewService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _workflowService = workflowService;
            _otpEmailService = otpEmailService;
            _smsSender = smsSender;
            _notifQueue = notifQueue;
            _legalReviewService = legalReviewService;
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

            // Notify both parties: negotiation updated
            var proj = await _context.Projects.FindAsync(model.ProjectId ?? 0);
            if (proj != null)
            {
                string msg = negotiation.GiaChotCuoiCung.HasValue
                    ? $"Giá chốt: {negotiation.GiaChotCuoiCung:N0} VNĐ. Đang chờ ký xác nhận."
                    : "Biên bản thương lượng đã được cập nhật.";

                // Notify buyer
                if (proj.CreatedBy.HasValue)
                    await _notifQueue.QueueAsync(proj.CreatedBy.Value, model.ProjectId,
                        "🤝 Đàm phán cập nhật", msg);

                // Notify seller (if selected)
                if (proj.SelectedSellerId.HasValue)
                    await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, model.ProjectId,
                        "🤝 Đàm phán cập nhật", msg);
            }

            return Redirect($"/Project/Details/{model.ProjectId}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /Negotiation/SaveInline  (AJAX — used by inline form in Step 5)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInline(int projectId, decimal? giaChotCuoiCung,
            string? dieuKhoanThanhToan, string? hinhThucKy, IFormFile? bienBanFile)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, _, _) = await GetAccessAsync(projectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });

                var negotiation = await _context.NegotiationForms
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);
                if (negotiation == null)
                    return Json(new { success = false, message = "Chưa có biên bản thương lượng." });

                if (negotiation.StatusId == (int)NegotiationStatus.Completed)
                    return Json(new { success = false, message = "Bước thương lượng đã hoàn tất." });

                // Handle file upload
                if (bienBanFile != null && bienBanFile.Length > 0)
                {
                    var allowed = new[] { ".pdf", ".doc", ".docx" };
                    var ext = Path.GetExtension(bienBanFile.FileName).ToLower();
                    if (!allowed.Contains(ext))
                        return Json(new { success = false, message = "Chỉ chấp nhận .pdf, .doc, .docx" });
                    if (bienBanFile.Length > 20 * 1024 * 1024)
                        return Json(new { success = false, message = "File không quá 20MB." });

                    string folder = Path.Combine(_environment.WebRootPath, "uploads", "negotiations");
                    Directory.CreateDirectory(folder);
                    string fname = $"{Guid.NewGuid()}_{bienBanFile.FileName}";
                    using var stream = new FileStream(Path.Combine(folder, fname), FileMode.Create);
                    await bienBanFile.CopyToAsync(stream);
                    negotiation.BienBanThuongLuongFile = $"/uploads/negotiations/{fname}";
                }

                negotiation.GiaChotCuoiCung    = giaChotCuoiCung;
                negotiation.DieuKhoanThanhToan = dieuKhoanThanhToan;
                negotiation.HinhThucKy         = hinhThucKy;
                negotiation.NguoiSua           = userId;
                negotiation.NgaySua            = DateTime.Now;

                if (hinhThucKy == "E-Sign" || hinhThucKy == "OTP")
                    negotiation.DaKySo = true;

                // If buyer re-edits after signatures were collected, reset them
                bool wasAlreadySigned = negotiation.SellerSigned || negotiation.BuyerSigned;
                if (wasAlreadySigned)
                {
                    negotiation.SellerSigned   = false;
                    negotiation.BuyerSigned    = false;
                    negotiation.SellerSignedAt = null;
                    negotiation.BuyerSignedAt  = null;
                }

                // Set/keep WaitingSignature when price is provided
                if (negotiation.GiaChotCuoiCung.HasValue)
                {
                    negotiation.StatusId = (int)NegotiationStatus.WaitingSignature;
                }

                await _context.SaveChangesAsync();

                // Notify both parties
                var proj = await _context.Projects.FindAsync(projectId);
                if (proj != null)
                {
                    string msg = negotiation.GiaChotCuoiCung.HasValue
                        ? $"Giá chốt: {negotiation.GiaChotCuoiCung:N0} VNĐ. Đang chờ ký xác nhận."
                        : "Biên bản thương lượng đã được cập nhật.";

                    if (proj.CreatedBy.HasValue)
                        await _notifQueue.QueueAsync(proj.CreatedBy.Value, projectId,
                            "🤝 Đàm phán cập nhật", msg);
                    if (proj.SelectedSellerId.HasValue)
                        await _notifQueue.QueueAsync(proj.SelectedSellerId.Value, projectId,
                            "🤝 Đàm phán cập nhật", msg);
                }

                return Json(new { success = true, message = "Đã lưu thông tin thương thảo!", hinhThucKy = negotiation.HinhThucKy, statusId = negotiation.StatusId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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

                // Get user email/phone
                var user = await _context.Users.FindAsync(userId);
                var email    = user?.Email ?? "";
                var phone    = user?.PhoneNumber ?? dto.PhoneNumber ?? "";
                var fullName = user?.FullName ?? user?.UserName ?? "Người dùng";
                var role     = isBuyer ? "Buyer" : "Seller";
                var channel  = dto.Channel?.ToLower() ?? "email";

                if (channel == "sms")
                {
                    if (string.IsNullOrEmpty(phone))
                        return Json(new { success = false, message = "Không tìm thấy số điện thoại. Vui lòng nhập số điện thoại." });

                    var smsMessage = $"[Techport] Mã OTP xác nhận ký Bước 5 (Dự án #{dto.ProjectId}): {otp}. Có hiệu lực 5 phút.";
                    await _smsSender.SendAsync(phone, smsMessage);
                    var maskedPhone = phone.Length > 4 ? new string('*', phone.Length - 4) + phone[^4..] : phone;
                    return Json(new { success = true, channel = "sms", message = $"OTP đã gửi đến SMS {maskedPhone}. Có hiệu lực 5 phút." });
                }
                else
                {
                    if (string.IsNullOrEmpty(email))
                        return Json(new { success = false, message = "Không tìm thấy email. Vui lòng liên hệ quản trị viên." });

                    await _otpEmailService.SendOtpAsync(email, fullName, otp, role, dto.ProjectId);
                    var maskedEmail = email.Length > 3 ? email[..3] + "***" + email[email.IndexOf('@')..] : email;
                    return Json(new { success = true, channel = "email", message = $"OTP đã gửi đến {maskedEmail}. Có hiệu lực 5 phút." });
                }
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
                        // Auto-create Contract Draft (Step 6)
                        _ = _legalReviewService.AutoCreateDraftAsync(dto.ProjectId);
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

        // ─────────────────────────────────────────────────────────────────────
        // POST /Negotiation/RequestESign  (AJAX) — E-Sign digital signature
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RequestESign([FromBody] RequestOtpDto dto)
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

                if (isSeller && negotiation.SellerSigned)
                    return Json(new { success = false, message = "Seller đã ký rồi." });
                if (isBuyer && negotiation.BuyerSigned)
                    return Json(new { success = false, message = "Buyer đã ký rồi." });

                // Mark as signed via E-Sign
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;
                    if (isSeller) { negotiation.SellerSigned = true; negotiation.SellerSignedAt = now; }
                    if (isBuyer)  { negotiation.BuyerSigned = true;  negotiation.BuyerSignedAt = now; }
                    negotiation.NguoiSua = userId;
                    negotiation.NgaySua = now;

                    bool bothSigned = negotiation.SellerSigned && negotiation.BuyerSigned;
                    if (bothSigned)
                    {
                        negotiation.StatusId = (int)NegotiationStatus.Completed;
                        await _context.SaveChangesAsync();
                        await _workflowService.CompleteStep(dto.ProjectId, 5);
                        // Auto-create Contract Draft (Step 6)
                        _ = _legalReviewService.AutoCreateDraftAsync(dto.ProjectId);
                    }
                    else
                    {
                        negotiation.StatusId = (int)NegotiationStatus.PartiallySigned;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    string msg = bothSigned
                        ? "Cả hai bên đã ký! Bước 5 hoàn tất. Bước 6 đã được mở."
                        : (isSeller ? "Seller đã ký E-Sign." : "Buyer đã ký E-Sign.") + " Đang chờ bên còn lại ký.";

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

        // ─────────────────────────────────────────────────────────────────────
        // POST /Negotiation/UploadSignedFile  (AJAX) — Upload signed document
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadSignedFile(int projectId, IFormFile signedFile)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canAccess, isBuyer, isSeller) = await GetAccessAsync(projectId, userId);
                if (!canAccess) return Json(new { success = false, message = "Không có quyền truy cập." });

                var negotiation = await _context.NegotiationForms
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId);
                if (negotiation == null)
                    return Json(new { success = false, message = "Chưa có biên bản thương lượng." });

                if (negotiation.StatusId == (int)NegotiationStatus.Completed)
                    return Json(new { success = false, message = "Bước thương lượng đã hoàn tất." });

                if (negotiation.StatusId < (int)NegotiationStatus.WaitingSignature)
                    return Json(new { success = false, message = "Cần thống nhất giá trước khi ký." });

                if (isSeller && negotiation.SellerSigned)
                    return Json(new { success = false, message = "Seller đã ký rồi." });
                if (isBuyer && negotiation.BuyerSigned)
                    return Json(new { success = false, message = "Buyer đã ký rồi." });

                if (signedFile == null || signedFile.Length == 0)
                    return Json(new { success = false, message = "Vui lòng chọn file." });

                if (signedFile.Length > 20 * 1024 * 1024)
                    return Json(new { success = false, message = "File quá lớn. Tối đa 20MB." });

                // Save file
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "negotiations", projectId.ToString());
                Directory.CreateDirectory(uploadsDir);
                var role = isBuyer ? "buyer" : "seller";
                var ext = Path.GetExtension(signedFile.FileName);
                var fileName = $"signed_{role}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await signedFile.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/negotiations/{projectId}/{fileName}";

                // Mark as signed
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;
                    if (isSeller) { negotiation.SellerSigned = true; negotiation.SellerSignedAt = now; }
                    if (isBuyer)  { negotiation.BuyerSigned = true;  negotiation.BuyerSignedAt = now; }
                    negotiation.NguoiSua = userId;
                    negotiation.NgaySua = now;

                    bool bothSigned = negotiation.SellerSigned && negotiation.BuyerSigned;
                    if (bothSigned)
                    {
                        negotiation.StatusId = (int)NegotiationStatus.Completed;
                        await _context.SaveChangesAsync();
                        await _workflowService.CompleteStep(projectId, 5);
                        // Auto-create Contract Draft (Step 6)
                        _ = _legalReviewService.AutoCreateDraftAsync(projectId);
                    }
                    else
                    {
                        negotiation.StatusId = (int)NegotiationStatus.PartiallySigned;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    string msg = bothSigned
                        ? "Cả hai bên đã ký! Bước 5 hoàn tất. Bước 6 đã được mở."
                        : (isSeller ? "Seller đã tải lên file ký." : "Buyer đã tải lên file ký.") + " Đang chờ bên còn lại ký.";

                    return Json(new { success = true, completed = bothSigned, message = msg, filePath = relativePath });
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

        // ─────────────────────────────────────────────────────────────────────
        // POST /Negotiation/ConfirmUploadFile  (AJAX) — Confirm already-uploaded file
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ConfirmUploadFile([FromBody] RequestOtpDto dto)
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

                if (string.IsNullOrEmpty(negotiation.BienBanThuongLuongFile))
                    return Json(new { success = false, message = "Chưa có file biên bản được tải lên. Vui lòng tải lên file trước." });

                if (isSeller && negotiation.SellerSigned)
                    return Json(new { success = false, message = "Seller đã xác nhận rồi." });
                if (isBuyer && negotiation.BuyerSigned)
                    return Json(new { success = false, message = "Buyer đã xác nhận rồi." });

                // Mark as signed
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;
                    if (isSeller) { negotiation.SellerSigned = true; negotiation.SellerSignedAt = now; }
                    if (isBuyer)  { negotiation.BuyerSigned = true;  negotiation.BuyerSignedAt = now; }
                    negotiation.NguoiSua = userId;
                    negotiation.NgaySua = now;

                    bool bothSigned = negotiation.SellerSigned && negotiation.BuyerSigned;
                    if (bothSigned)
                    {
                        negotiation.StatusId = (int)NegotiationStatus.Completed;
                        await _context.SaveChangesAsync();
                        await _workflowService.CompleteStep(dto.ProjectId, 5);
                        // Auto-create Contract Draft (Step 6)
                        _ = _legalReviewService.AutoCreateDraftAsync(dto.ProjectId);
                    }
                    else
                    {
                        negotiation.StatusId = (int)NegotiationStatus.PartiallySigned;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    string msg = bothSigned
                        ? "Cả hai bên đã xác nhận! Bước 5 hoàn tất. Bước 6 đã được mở."
                        : (isSeller ? "Seller đã xác nhận." : "Buyer đã xác nhận.") + " Đang chờ bên còn lại xác nhận.";

                    return Json(new { success = true, completed = bothSigned, message = msg });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Lỗi khi xác nhận: " + ex.Message });
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
    public class RequestOtpDto { public int ProjectId { get; set; } public string? Channel { get; set; } public string? PhoneNumber { get; set; } }
    public class VerifyOtpDto  { public int ProjectId { get; set; } public string Otp { get; set; } = ""; }
}
