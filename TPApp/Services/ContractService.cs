using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class ContractService : IContractService
    {
        private readonly AppDbContext              _context;
        private readonly IHashService              _hash;
        private readonly IContractAuditService     _audit;
        private readonly IContractApprovalService  _approval;
        private readonly IWorkflowService          _workflow;
        private readonly IWebHostEnvironment       _env;
        private readonly ILogger<ContractService>  _logger;

        public ContractService(AppDbContext context, IHashService hash,
            IContractAuditService audit, IContractApprovalService approval,
            IWorkflowService workflow, IWebHostEnvironment env,
            ILogger<ContractService> logger)
        {
            _context  = context;
            _hash     = hash;
            _audit    = audit;
            _approval = approval;
            _workflow = workflow;
            _env      = env;
            _logger   = logger;
        }

        // ─── Auto-create draft from negotiation data ──────────────────────────
        public async Task<ProjectContract> AutoCreateDraftAsync(int projectId, int createdByUserId)
        {
            await ArchiveActiveAsync(projectId);

            var neg  = await _context.NegotiationForms.FirstOrDefaultAsync(n => n.ProjectId == projectId);
            var proj = await _context.Projects.FindAsync(projectId);

            // Bên B: Buyer – thông tin từ Step 1 (TechTransferRequest)
            var step1 = await _context.TechTransferRequests
                .Where(r => r.ProjectId == projectId)
                .OrderByDescending(r => r.NgayTao)
                .FirstOrDefaultAsync();

            // Bên A: Seller – thông tin user
            ApplicationUser? sellerUser = null;
            if (proj?.SelectedSellerId.HasValue == true)
                sellerUser = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == proj.SelectedSellerId.Value);

            int ver  = await NextVersionAsync(projectId);
            var html = BuildHtmlSnapshot(proj, neg, step1, sellerUser, ver, _env);

            var contract = new ProjectContract
            {
                ProjectId     = projectId,
                VersionNumber = ver,
                SourceType    = 1, // AutoGenerate
                Title         = $"HỢP ĐỒNG CHUYỂN GIAO CÔNG NGHỆ – {proj?.ProjectName ?? $"Dự án #{projectId}"} (v{ver})",
                StatusId      = (int)ContractStatus.Draft,
                HtmlContent   = html,
                IsActive      = true,
                CreatedBy     = createdByUserId,
                CreatedDate   = DateTime.UtcNow
            };

            _context.ProjectContracts.Add(contract);
            await _context.SaveChangesAsync();

            await _audit.AppendAsync("ProjectContract", contract.Id.ToString(), "AutoDraftCreated",
                new { projectId, ver }, createdByUserId);

            return contract;
        }

        // ─── Upload contract file ─────────────────────────────────────────────
        public async Task<ProjectContract> UploadDraftAsync(int projectId, int userId, IFormFile file, IWebHostEnvironment env)
        {
            ValidateFile(file);
            await ArchiveActiveAsync(projectId);

            var proj = await _context.Projects.FindAsync(projectId);
            int ver  = await NextVersionAsync(projectId);

            var (path, storedName) = await SaveFileAsync(file, projectId, ver, env);

            string sha256;
            await using (var fs = System.IO.File.OpenRead(path))
                sha256 = _hash.ComputeSha256(fs);

            var contract = new ProjectContract
            {
                ProjectId        = projectId,
                VersionNumber    = ver,
                SourceType       = 2, // Upload
                Title            = $"HỢP ĐỒNG – {proj?.ProjectName ?? $"Dự án #{projectId}"} (v{ver})",
                StatusId         = (int)ContractStatus.Draft,
                OriginalFilePath = path,
                OriginalFileName = storedName,
                Sha256Original   = sha256,
                IsActive         = true,
                CreatedBy        = userId,
                CreatedDate      = DateTime.UtcNow
            };

            _context.ProjectContracts.Add(contract);
            await _context.SaveChangesAsync();

            await _audit.AppendAsync("ProjectContract", contract.Id.ToString(), "UploadDraft",
                new { projectId, ver, sha256 }, userId);

            return contract;
        }

        // ─── Get active contract ──────────────────────────────────────────────
        public async Task<ProjectContract?> GetActiveContractAsync(int projectId)
            => await _context.ProjectContracts
                    .Where(c => c.ProjectId == projectId && c.IsActive)
                    .OrderByDescending(c => c.VersionNumber)
                    .FirstOrDefaultAsync();

        public async Task<List<ProjectContract>> GetAllVersionsAsync(int projectId)
            => await _context.ProjectContracts
                    .Where(c => c.ProjectId == projectId)
                    .OrderByDescending(c => c.VersionNumber)
                    .ToListAsync();

        // ─── Revise contract (create new version, archive old) ────────────────
        public async Task<ProjectContract> ReviseContractAsync(int contractId, int userId, IFormFile file, IWebHostEnvironment env)
        {
            var old = await _context.ProjectContracts.FindAsync(contractId)
                      ?? throw new InvalidOperationException("Contract not found.");

            if (old.StatusId >= (int)ContractStatus.ReadyToSign)
                throw new InvalidOperationException("Không thể chỉnh sửa sau khi ReadyToSign.");

            return await UploadDraftAsync(old.ProjectId, userId, file, env);
        }

        // ─── Set ReadyToSign ─────────────────────────────────────────────────
        public async Task<(bool ok, string message)> SetReadyToSignAsync(int contractId, int userId)
        {
            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract == null) return (false, "Không tìm thấy hợp đồng.");

            if (contract.StatusId >= (int)ContractStatus.ReadyToSign)
                return (false, "Hợp đồng đã ở trạng thái ReadyToSign hoặc cao hơn.");

            if (string.IsNullOrEmpty(contract.HtmlContent) && string.IsNullOrEmpty(contract.OriginalFilePath))
                return (false, "Hợp đồng chưa có nội dung. Vui lòng upload file hoặc tạo auto-draft.");

            bool allApproved = await _approval.AllPartiesApprovedAsync(contractId);
            if (!allApproved)
                return (false, "Chưa đủ 3 bên phê duyệt (Buyer + Seller + Tư vấn).");

            contract.StatusId      = (int)ContractStatus.ReadyToSign;
            contract.ReadyToSignAt = DateTime.UtcNow;
            contract.ModifiedBy    = userId;
            contract.ModifiedDate  = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Complete Step 6 → unlock Step 7
            await _workflow.CompleteStep(contract.ProjectId, 6);

            await _audit.AppendAsync("ProjectContract", contractId.ToString(), "ReadyToSign",
                new { userId }, userId);

            return (true, "✅ Hợp đồng đã được chốt ReadyToSign. Bước 7 đã mở.");
        }

        // ─── Secure file download ─────────────────────────────────────────────
        public async Task<(string? FilePath, string? FileName)> GetDownloadOriginalAsync(int contractId, int userId)
        {
            var c = await _context.ProjectContracts.FindAsync(contractId);
            return (c?.OriginalFilePath, c?.OriginalFileName);
        }

        public async Task<(string? FilePath, string? FileName)> GetDownloadSignedAsync(int contractId, int userId)
        {
            var c = await _context.ProjectContracts.FindAsync(contractId);
            return (c?.SignedFilePath, c?.SignedFileName);
        }

        // ─── Private helpers ──────────────────────────────────────────────────
        private async Task ArchiveActiveAsync(int projectId)
        {
            var active = await _context.ProjectContracts
                .Where(c => c.ProjectId == projectId && c.IsActive)
                .ToListAsync();

            foreach (var c in active)
            {
                c.IsActive   = false;
                c.ArchivedAt = DateTime.UtcNow;
                if (c.StatusId < (int)ContractStatus.ReadyToSign)
                    c.StatusId = (int)ContractStatus.Archived;
            }
        }

        private async Task<int> NextVersionAsync(int projectId)
        {
            var max = await _context.ProjectContracts
                .Where(c => c.ProjectId == projectId)
                .MaxAsync(c => (int?)c.VersionNumber) ?? 0;
            return max + 1;
        }

        private static async Task<(string Path, string StoredName)> SaveFileAsync(
            IFormFile file, int projectId, int version, IWebHostEnvironment env)
        {
            var root = System.IO.Path.Combine(env.ContentRootPath, "wwwroot", "uploads", "contracts", $"proj_{projectId}");
            System.IO.Directory.CreateDirectory(root);

            var ext        = System.IO.Path.GetExtension(file.FileName);
            var storedName = $"contract_v{version}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var fullPath   = System.IO.Path.Combine(root, storedName);

            await using var fs = System.IO.File.Create(fullPath);
            await file.CopyToAsync(fs);

            return (fullPath, storedName);
        }

        private static void ValidateFile(IFormFile file)
        {
            var allowed = new[] { ".pdf", ".docx" };
            var ext     = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                throw new InvalidOperationException("Chỉ chấp nhận file .pdf / .docx.");
            if (file.Length > 25 * 1024 * 1024)
                throw new InvalidOperationException("File vượt quá 25 MB.");
        }

        // ─── Build HTML từ template file wwwroot/templates/ ───────────────────
        private static string BuildHtmlSnapshot(
            Project?             proj,
            NegotiationForm?     neg,
            TechTransferRequest? step1,
            ApplicationUser?     sellerUser,
            int                  ver,
            IWebHostEnvironment  env)
        {
            var today = DateTime.Now;

            // Bên A – Seller (bên chuyển giao)
            var benATen       = sellerUser?.FullName ?? sellerUser?.UserName ?? "——";
            var benADienThoai = sellerUser?.PhoneNumber ?? "——";
            var benADaiDien   = sellerUser?.FullName ?? sellerUser?.UserName ?? "——";

            // Bên B – Buyer (bên nhận chuyển giao, từ Step 1)
            var benBTen       = step1?.DonVi ?? step1?.HoTen ?? "——";
            var benBTruSo     = step1?.DiaChi ?? "——";
            var benBDienThoai = step1?.DienThoai ?? "——";
            var benBDaiDien   = step1?.HoTen ?? "——";
            var benBChucVu    = step1?.ChucVu ?? "——";

            // Thông tin hợp đồng
            var soHD        = $"TechPort-{proj?.Id ?? 0}-v{ver}-{today:yyyyMM}";
            var ngayHD      = $"ngày {today.Day} tháng {today.Month} năm {today.Year}";
            var projectName = proj?.ProjectName ?? "——";
            var tenCongNghe = step1?.TenCongNghe ?? proj?.ProjectName ?? "——";
            var moTa        = step1?.MoTaNhuCau ?? "——";
            var tongGia     = neg?.GiaChotCuoiCung != null
                                ? neg.GiaChotCuoiCung.Value.ToString("N0") + " VNĐ"
                                : "——";
            var ptThanhToan = neg?.DieuKhoanThanhToan ?? "——";

            // Đọc template từ wwwroot/templates/
            var templatePath = System.IO.Path.Combine(
                env.WebRootPath, "templates", "contract_chuyen_giao_cong_nghe.html");

            var html = System.IO.File.Exists(templatePath)
                ? System.IO.File.ReadAllText(templatePath, System.Text.Encoding.UTF8)
                : "<p><strong>Lỗi:</strong> Không tìm thấy template hợp đồng tại wwwroot/templates/contract_chuyen_giao_cong_nghe.html</p>";

            // Thay thế placeholder
            return html
                .Replace("{{SO_HD}}",                   soHD)
                .Replace("{{NGAY_HD}}",                 ngayHD)
                .Replace("{{PROJECT_NAME}}",             projectName)
                .Replace("{{TEN_CONG_NGHE}}",           tenCongNghe)
                .Replace("{{MO_TA}}",                   moTa)
                .Replace("{{TONG_GIA}}",                tongGia)
                .Replace("{{PHUONG_THUC_THANH_TOAN}}", ptThanhToan)
                .Replace("{{BEN_A_TEN}}",               benATen)
                .Replace("{{BEN_A_TRU_SO}}",            "——")
                .Replace("{{BEN_A_DIEN_THOAI}}",        benADienThoai)
                .Replace("{{BEN_A_MA_SO_THUE}}",        "——")
                .Replace("{{BEN_A_TAI_KHOAN}}",         "——")
                .Replace("{{BEN_A_DAI_DIEN}}",          benADaiDien)
                .Replace("{{BEN_A_CHUC_VU}}",           "——")
                .Replace("{{BEN_B_TEN}}",               benBTen)
                .Replace("{{BEN_B_TRU_SO}}",            benBTruSo)
                .Replace("{{BEN_B_DIEN_THOAI}}",        benBDienThoai)
                .Replace("{{BEN_B_MA_SO_THUE}}",        "——")
                .Replace("{{BEN_B_TAI_KHOAN}}",         "——")
                .Replace("{{BEN_B_DAI_DIEN}}",          benBDaiDien)
                .Replace("{{BEN_B_CHUC_VU}}",           benBChucVu)
                .Replace("{{VERSION}}",                 $"v{ver}")
                .Replace("{{NGAY_TAO}}",                today.ToString("dd/MM/yyyy HH:mm"));
        }
    }
}
