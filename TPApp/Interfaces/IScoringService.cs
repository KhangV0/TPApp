using TPApp.Entities;
using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Service for managing consultant scoring of proposals
    /// </summary>
    public interface IScoringService
    {
        /// <summary>
        /// Check if consultant can score a proposal
        /// Guards: User is consultant on project, proposal is submitted, step 4 in progress, not own proposal
        /// </summary>
        Task<bool> CanScoreProposalAsync(int proposalId, int consultantId);

        /// <summary>
        /// Score a proposal (create or update)
        /// </summary>
        Task<ProposalScore> ScoreProposalAsync(int proposalId, int consultantId, ProposalScoreDto dto);

        /// <summary>
        /// Get all scores for a proposal
        /// </summary>
        Task<List<ProposalScore>> GetProposalScoresAsync(int proposalId);

        /// <summary>
        /// Get average overall score for a proposal
        /// </summary>
        Task<decimal> GetAverageScoreAsync(int proposalId);

        /// <summary>
        /// Get consultant's score for a proposal (if exists)
        /// </summary>
        Task<ProposalScore?> GetConsultantScoreAsync(int proposalId, int consultantId);
    }
}
