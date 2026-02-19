using TPApp.Entities;

namespace TPApp.Interfaces
{
    public interface IContractApprovalService
    {
        /// <summary>Add or update approval decision for the given role on the active contract.</summary>
        Task<(bool ok, string message)> SubmitDecisionAsync(
            int contractId, int userId, int role, bool approved, string? comment,
            string? ipAddress, string? userAgent);

        /// <summary>Get all approval records for a contract version.</summary>
        Task<List<ContractApproval>> GetApprovalSummaryAsync(int contractId);

        /// <summary>Returns true only when Buyer(1) + Seller(2) + Consultant(3) all Approved.</summary>
        Task<bool> AllPartiesApprovedAsync(int contractId);
    }
}
