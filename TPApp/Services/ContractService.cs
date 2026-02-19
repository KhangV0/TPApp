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
        private readonly ILogger<ContractService>  _logger;

        public ContractService(AppDbContext context, IHashService hash,
            IContractAuditService audit, IContractApprovalService approval,
            IWorkflowService workflow, ILogger<ContractService> logger)
        {
            _context  = context;
            _hash     = hash;
            _audit    = audit;
            _approval = approval;
            _workflow = workflow;
            _logger   = logger;
        }

        // ─── Auto-create draft from negotiation data ─────────────────────────
        public async Task<ProjectContract> AutoCreateDraftAsync(int projectId, int createdByUserId)
        {
            // Archive any previous active version
            await ArchiveActiveAsync(projectId);

            var neg = await _context.NegotiationForms.FirstOrDefaultAsync(n => n.ProjectId == projectId);
            var proj = await _context.Projects.FindAsync(projectId);

            int ver = await NextVersionAsync(projectId);

            var html = BuildHtmlSnapshot(proj, neg);

            var contract = new ProjectContract
            {
                ProjectId     = projectId,
                VersionNumber = ver,
                SourceType    = 1, // AutoGenerate
                Title         = $"HỢP ĐỒNG THƯƠNG MẠI – {proj?.ProjectName ?? $"Dự án #{projectId}"} (v{ver})",
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

            // Validate: must have either HTML or file
            if (string.IsNullOrEmpty(contract.HtmlContent) && string.IsNullOrEmpty(contract.OriginalFilePath))
                return (false, "Hợp đồng chưa có nội dung. Vui lòng upload file hoặc tạo auto-draft.");

            // Validate all parties approved
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
                c.IsActive    = false;
                c.ArchivedAt  = DateTime.UtcNow;
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

            var ext = System.IO.Path.GetExtension(file.FileName);
            var storedName = $"contract_v{version}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var fullPath = System.IO.Path.Combine(root, storedName);

            await using var fs = System.IO.File.Create(fullPath);
            await file.CopyToAsync(fs);

            return (fullPath, storedName);
        }

        private static void ValidateFile(IFormFile file)
        {
            var allowed = new[] { ".pdf", ".docx" };
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                throw new InvalidOperationException($"Chỉ chấp nhận file .pdf / .docx.");
            if (file.Length > 25 * 1024 * 1024)
                throw new InvalidOperationException("File vượt quá 25 MB.");
        }

        private static string BuildHtmlSnapshot(Project? proj, NegotiationForm? neg)
        {
            var price       = neg?.GiaChotCuoiCung?.ToString("N0") ?? "—";
            var payment     = neg?.DieuKhoanThanhToan ?? "—";
            var projectName = proj?.ProjectName ?? "—";

            return $@"<div style=""font-family:Arial,sans-serif;max-width:900px;margin:auto;padding:32px;border:1px solid #ccc;border-radius:8px"">
  <h2 style=""text-align:center;color:#1a3c6e"">HỢP ĐỒNG CHUYỂN GIAO CÔNG NGHỆ</h2>
  <p style=""text-align:center;color:#666"">Bản nháp tự động từ kết quả đàm phán Bước 5</p>
  <hr/>
  <h4>Điều 1 – Đối tượng hợp đồng</h4>
  <p>Dự án: <strong>{projectName}</strong></p>
  <h4>Điều 2 – Giá trị hợp đồng</h4>
  <p>Giá thỏa thuận: <strong>{price} VNĐ</strong></p>
  <h4>Điều 3 – Điều khoản thanh toán</h4>
  <p>{payment}</p>
  <h4>Điều 4 – Thời gian giao hàng</h4>
  <p>Theo thỏa thuận của hai bên.</p>
  <h4>Điều 5 – Điều khoản pháp lý</h4>
  <p>Hợp đồng điện tử có giá trị pháp lý tương đương văn bản giấy sau khi 2 bên ký số
     theo quy định Luật Giao dịch điện tử 2023.</p>
  <hr/>
  <p style=""color:#aaa;font-size:11px"">
    Tài liệu tự động – Phiên bản nháp. Cần rà soát pháp lý trước khi ký.
    Tạo lúc: {DateTime.Now:dd/MM/yyyy HH:mm}
  </p>
</div>";
        }
    }
}
