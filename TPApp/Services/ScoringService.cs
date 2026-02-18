using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    /// <summary>
    /// Service for managing consultant scoring of proposals
    /// </summary>
    public class ScoringService : IScoringService
    {
        private readonly AppDbContext _context;

        public ScoringService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanScoreProposalAsync(int proposalId, int consultantId)
        {
            // Guard 1: Proposal must exist and be submitted
            var proposal = await _context.ProposalSubmissions
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null || proposal.StatusId != (int)ProposalStatus.Submitted)
            {
                return false;
            }

            // Guard 2: User must be a consultant on the project
            var isConsultant = await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == proposal.ProjectId &&
                              m.UserId == consultantId &&
                              m.Role == 3); // 3 = Consultant

            if (!isConsultant)
            {
                return false;
            }

            // Guard 3: Cannot score own proposal (if consultant is also seller)
            if (proposal.NguoiTao == consultantId)
            {
                return false;
            }

            // Guard 4: Step 4 must be in progress (optional - can be removed if too strict)
            // Commenting out for now as it may block legitimate scoring
            // var step4 = await _context.ProjectStepStates
            //     .FirstOrDefaultAsync(s => s.ProjectId == proposal.ProjectId && s.StepNumber == 4);
            // if (step4 == null || step4.Status != 1) // 1 = InProgress
            // {
            //     return false;
            // }

            return true;
        }

        public async Task<ProposalScore> ScoreProposalAsync(int proposalId, int consultantId, ProposalScoreDto dto)
        {
            if (!await CanScoreProposalAsync(proposalId, consultantId))
            {
                throw new InvalidOperationException("Cannot score this proposal. Check consultant permissions and proposal status.");
            }

            // Check if consultant already scored this proposal
            var existingScore = await _context.ProposalScores
                .FirstOrDefaultAsync(s => s.ProposalId == proposalId && s.ConsultantId == consultantId);

            ProposalScore score;

            if (existingScore != null)
            {
                // Update existing score
                existingScore.TechnicalScore = dto.TechnicalScore;
                existingScore.PriceScore = dto.PriceScore;
                existingScore.TimelineScore = dto.TimelineScore;
                existingScore.OverallScore = dto.OverallScore;
                existingScore.Comments = dto.Comments;
                existingScore.UpdatedDate = DateTime.Now;

                score = existingScore;
            }
            else
            {
                // Create new score
                score = new ProposalScore
                {
                    ProposalId = proposalId,
                    ConsultantId = consultantId,
                    TechnicalScore = dto.TechnicalScore,
                    PriceScore = dto.PriceScore,
                    TimelineScore = dto.TimelineScore,
                    OverallScore = dto.OverallScore,
                    Comments = dto.Comments,
                    CreatedDate = DateTime.Now
                };

                _context.ProposalScores.Add(score);
            }

            await _context.SaveChangesAsync();

            return score;
        }

        public async Task<List<ProposalScore>> GetProposalScoresAsync(int proposalId)
        {
            // Get only the latest score from each consultant (in case they updated their score)
            var allScores = await _context.ProposalScores
                .Include(s => s.Consultant)
                .Where(s => s.ProposalId == proposalId)
                .OrderByDescending(s => s.UpdatedDate ?? s.CreatedDate)
                .ToListAsync();

            // Group by consultant and take the latest score for each
            return allScores
                .GroupBy(s => s.ConsultantId)
                .Select(g => g.First())
                .OrderByDescending(s => s.UpdatedDate ?? s.CreatedDate)
                .ToList();
        }

        public async Task<decimal> GetAverageScoreAsync(int proposalId)
        {
            var scores = await _context.ProposalScores
                .Where(s => s.ProposalId == proposalId)
                .ToListAsync();

            if (scores.Count == 0)
            {
                return 0;
            }

            // Calculate average of all 4 scores for each consultant, then average across consultants
            var consultantAverages = scores.Select(s =>
            {
                var scoreValues = new List<decimal>();
                
                if (s.TechnicalScore.HasValue) scoreValues.Add(s.TechnicalScore.Value);
                if (s.PriceScore.HasValue) scoreValues.Add(s.PriceScore.Value);
                if (s.TimelineScore.HasValue) scoreValues.Add(s.TimelineScore.Value);
                if (s.OverallScore.HasValue) scoreValues.Add(s.OverallScore.Value);
                
                return scoreValues.Count > 0 ? scoreValues.Average() : 0m;
            }).Where(avg => avg > 0).ToList();

            return consultantAverages.Count > 0 ? consultantAverages.Average() : 0m;
        }

        public async Task<ProposalScore?> GetConsultantScoreAsync(int proposalId, int consultantId)
        {
            return await _context.ProposalScores
                .FirstOrDefaultAsync(s => s.ProposalId == proposalId && s.ConsultantId == consultantId);
        }
    }
}
