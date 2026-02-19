using TPApp.Entities;

namespace TPApp.Interfaces
{
    public interface IContractService
    {
        /// <summary>Auto-create draft from negotiation data (Step 5 → Step 6).</summary>
        Task<ProjectContract> AutoCreateDraftAsync(int projectId, int createdByUserId);

        /// <summary>Upload a contract file and create a new contract version.</summary>
        Task<ProjectContract> UploadDraftAsync(int projectId, int userId, IFormFile file, IWebHostEnvironment env);

        /// <summary>Get the active contract version for a project.</summary>
        Task<ProjectContract?> GetActiveContractAsync(int projectId);

        /// <summary>Get all versions of contracts for a project.</summary>
        Task<List<ProjectContract>> GetAllVersionsAsync(int projectId);

        /// <summary>Create a new revision (archives old active version).</summary>
        Task<ProjectContract> ReviseContractAsync(int contractId, int userId, IFormFile file, IWebHostEnvironment env);

        /// <summary>Freeze contract; validate all approvals and mark ReadyToSign. Completes Step 6.</summary>
        Task<(bool ok, string message)> SetReadyToSignAsync(int contractId, int userId);

        /// <summary>Download the original contract file path safely.</summary>
        Task<(string? FilePath, string? FileName)> GetDownloadOriginalAsync(int contractId, int userId);

        /// <summary>Download the signed contract file path safely.</summary>
        Task<(string? FilePath, string? FileName)> GetDownloadSignedAsync(int contractId, int userId);
    }
}
