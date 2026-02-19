using TPApp.Entities;
using TPApp.Enums;

namespace TPApp.Interfaces
{
    public interface ILegalReviewService
    {
        /// <summary>
        /// Auto-creates a draft LegalReviewForm (v1) from negotiation data
        /// when Step 5 is completed. Idempotent — skips if draft already exists.
        /// </summary>
        Task<LegalReviewForm?> AutoCreateDraftAsync(int projectId);

        /// <summary>Approve the contract draft — sets status to Completed.</summary>
        Task<bool> ApproveAsync(int projectId, int userId);

        /// <summary>Request changes — sets status to ChangesRequested.</summary>
        Task<bool> RejectAsync(int projectId, int userId, string reason);

        /// <summary>Add a review comment to the contract.</summary>
        Task<ContractComment?> AddCommentAsync(int projectId, int userId,
            string authorName, string text, ContractCommentType type);

        /// <summary>Mark a comment as resolved.</summary>
        Task<bool> ResolveCommentAsync(int commentId, int userId);

        /// <summary>Upload a new revision of the contract file.</summary>
        Task<bool> UploadRevisionAsync(int projectId, int userId, IFormFile file,
            IWebHostEnvironment environment);

        /// <summary>Check if Step 6 is completed (used to gate Step 7).</summary>
        Task<bool> IsCompletedAsync(int projectId);
    }
}
