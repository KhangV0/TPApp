using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class ContractApprovalService : IContractApprovalService
    {
        private readonly AppDbContext          _context;
        private readonly IContractAuditService _audit;
        private readonly ILogger<ContractApprovalService> _logger;

        public ContractApprovalService(AppDbContext context, IContractAuditService audit,
                                       ILogger<ContractApprovalService> logger)
        {
            _context = context;
            _audit   = audit;
            _logger  = logger;
        }

        public async Task<(bool ok, string message)> SubmitDecisionAsync(
            int contractId, int userId, int role, bool approved, string? comment,
            string? ipAddress, string? userAgent)
        {
            var contract = await _context.ProjectContracts.FindAsync(contractId);
            if (contract == null) return (false, "Không tìm thấy hợp đồng.");

            if (contract.StatusId >= (int)ContractStatus.ReadyToSign)
                return (false, "Hợp đồng đã đóng băng, không thể thay đổi phê duyệt.");

            // Upsert: one record per (contractId, role)
            var existing = await _context.ContractApprovals
                .FirstOrDefaultAsync(a => a.ContractId == contractId && a.Role == role);

            if (existing != null)
            {
                existing.StatusId  = approved ? (int)ContractApprovalStatus.Approved : (int)ContractApprovalStatus.Rejected;
                existing.Comment   = comment;
                existing.DecisionAt = DateTime.UtcNow;
                existing.IPAddress  = ipAddress;
                existing.UserAgent  = userAgent;
            }
            else
            {
                _context.ContractApprovals.Add(new ContractApproval
                {
                    ContractId  = contractId,
                    UserId      = userId,
                    Role        = role,
                    StatusId    = approved ? (int)ContractApprovalStatus.Approved : (int)ContractApprovalStatus.Rejected,
                    Comment     = comment,
                    DecisionAt  = DateTime.UtcNow,
                    IPAddress   = ipAddress,
                    UserAgent   = userAgent,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // If anyone rejects, push contract back to WaitingPartyReview
            if (!approved && contract.StatusId < (int)ContractStatus.ReadyToSign)
                contract.StatusId = (int)ContractStatus.WaitingPartyReview;

            await _context.SaveChangesAsync();

            var action = approved ? "Approved" : "Rejected";
            await _audit.AppendAsync("ContractApproval", contractId.ToString(), action,
                new { role, comment, approved }, userId, ipAddress);

            return (true, approved ? "✅ Đã duyệt hợp đồng." : "🔄 Đã yêu cầu chỉnh sửa.");
        }

        public async Task<List<ContractApproval>> GetApprovalSummaryAsync(int contractId)
            => await _context.ContractApprovals
                    .Where(a => a.ContractId == contractId)
                    .OrderBy(a => a.Role)
                    .ToListAsync();

        public async Task<bool> AllPartiesApprovedAsync(int contractId)
        {
            var approvals = await _context.ContractApprovals
                .Where(a => a.ContractId == contractId)
                .ToListAsync();

            // Require Buyer(1) + Seller(2) + Consultant(3) all approved
            var required = new[] { 1, 2, 3 };
            return required.All(r =>
                approvals.Any(a => a.Role == r && a.StatusId == (int)ContractApprovalStatus.Approved));
        }
    }
}
