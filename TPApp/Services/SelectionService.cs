using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    /// <summary>
    /// Service for managing seller selection and proposal approval
    /// </summary>
    public class SelectionService : ISelectionService
    {
        private readonly AppDbContext _context;
        private readonly IWorkflowService _workflowService;

        public SelectionService(AppDbContext context, IWorkflowService workflowService)
        {
            _context = context;
            _workflowService = workflowService;
        }

        public async Task<bool> CanSelectSellerAsync(int projectId, int proposalId, int buyerUserId)
        {
            // Guard 1: User must be project owner (buyer)
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.CreatedBy != buyerUserId)
            {
                return false;
            }

            // Guard 2: Proposal must exist and be submitted
            var proposal = await _context.ProposalSubmissions.FindAsync(proposalId);
            if (proposal == null || 
                proposal.ProjectId != projectId || 
                proposal.StatusId != (int)ProposalStatus.Submitted)
            {
                return false;
            }

            // Guard 3: Step 4 must be in progress
            var step4 = await _context.ProjectStepStates
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == 4);

            if (step4 == null || step4.Status != 1) // 1 = InProgress
            {
                return false;
            }

            // Guard 4: Seller must have valid accepted invitation
            var sellerId = proposal.NguoiTao;
            if (sellerId == null) return false;

            var invitation = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && 
                                         i.SellerId == sellerId && 
                                         i.IsActive &&
                                         i.StatusId == (int)RFQInvitationStatus.ProposalSubmitted);

            if (invitation == null)
            {
                return false;
            }

            return true;
        }

        public async Task SelectSellerAsync(int projectId, int proposalId, int buyerUserId)
        {
            if (!await CanSelectSellerAsync(projectId, proposalId, buyerUserId))
            {
                throw new InvalidOperationException("Cannot select this seller. Check proposal status and authorization.");
            }

            var proposal = await _context.ProposalSubmissions.FindAsync(proposalId);
            if (proposal == null || proposal.NguoiTao == null)
            {
                throw new InvalidOperationException("Proposal not found");
            }

            var sellerId = proposal.NguoiTao.Value;

            // 1. Set proposal as Selected
            proposal.StatusId = (int)ProposalStatus.Selected;

            // 2. Set Project.SelectedSellerId (THIS IS THE ONLY PLACE WHERE THIS HAPPENS)
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                throw new InvalidOperationException("Project not found");
            }

            project.SelectedSellerId = sellerId;
            project.SelectedDate = DateTime.Now;

            // 3. Reject all other proposals
            var otherProposals = await _context.ProposalSubmissions
                .Where(p => p.ProjectId == projectId && 
                           p.Id != proposalId && 
                           p.StatusId == (int)ProposalStatus.Submitted)
                .ToListAsync();

            foreach (var other in otherProposals)
            {
                other.StatusId = (int)ProposalStatus.Rejected;
            }

            await _context.SaveChangesAsync();

            // 4. Transition workflow to Step 5 (Negotiation)
            // TODO: Implement workflow transition methods in IWorkflowService
            // await _workflowService.CompleteStepAsync(projectId, 4, buyerUserId);
            // await _workflowService.StartStepAsync(projectId, 5, buyerUserId);
        }

        public async Task<int?> GetSelectedSellerIdAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            return project?.SelectedSellerId;
        }

        public async Task<bool> HasSelectedSellerAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            return project?.SelectedSellerId != null;
        }
    }
}
