using TPApp.Entities;
using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Service for managing proposal submissions with strict guards
    /// </summary>
    public interface IProposalService
    {
        /// <summary>
        /// Check if seller can submit proposal (has accepted invitation, NDA signed, not expired)
        /// </summary>
        Task<bool> CanSubmitProposalAsync(int projectId, int sellerId);

        /// <summary>
        /// Submit a new proposal or update existing draft
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when proposal cannot be submitted</exception>
        Task<ProposalSubmission> SubmitProposalAsync(int projectId, int sellerId, ProposalSubmissionDto dto);

        /// <summary>
        /// Get all submitted proposals for a project
        /// </summary>
        Task<List<ProposalSubmission>> GetProjectProposalsAsync(int projectId);

        /// <summary>
        /// Get seller's proposal for a project
        /// </summary>
        Task<ProposalSubmission?> GetSellerProposalAsync(int projectId, int sellerId);

        /// <summary>
        /// Check if seller has submitted proposal for project
        /// </summary>
        Task<bool> HasSubmittedProposalAsync(int projectId, int sellerId);
    }
}
