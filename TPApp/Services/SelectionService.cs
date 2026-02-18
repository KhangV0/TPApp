using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
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

            // Guard 2: Proposal must exist and belong to this project
            var proposal = await _context.ProposalSubmissions.FindAsync(proposalId);
            if (proposal == null || proposal.ProjectId != projectId)
            {
                return false;
            }

            // Guard 3: Proposal must be in Submitted status (not already selected/rejected)
            if (proposal.StatusId != (int)ProposalStatus.Submitted)
            {
                return false;
            }

            // Guard 4: Seller must have a valid invitation for this project
            var sellerId = proposal.NguoiTao;
            if (sellerId == null) return false;

            var invitation = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && 
                                         i.SellerId == sellerId && 
                                         i.IsActive);

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

            // 4. Clean up ProjectMembers - only selected seller remains active
            // Deactivate all Seller role members
            var sellerMembers = await _context.ProjectMembers
                .Where(m => m.ProjectId == projectId && m.Role == 2) // Role 2 = Seller
                .ToListAsync();

            foreach (var member in sellerMembers)
            {
                member.IsActive = false;
            }

            // Add or reactivate selected seller as ProjectMember
            var selectedSellerMember = sellerMembers.FirstOrDefault(m => m.UserId == sellerId);
            if (selectedSellerMember != null)
            {
                // Reactivate if exists
                selectedSellerMember.IsActive = true;
            }
            else
            {
                // Add new member
                _context.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = sellerId,
                    Role = 2, // Seller
                    JoinedDate = DateTime.Now,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();

            // 5. Transition workflow: Complete Step 4, Start Step 5
            var step4 = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == 4);
            if (step4 != null)
            {
                step4.StatusId = 2; // Completed
                step4.CompletedDate = DateTime.Now;
            }

            var step5 = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == 5);
            if (step5 != null)
            {
                step5.StatusId = 1; // InProgress
            }
            else
            {
                // Create Step 5 if it doesn't exist
                _context.ProjectSteps.Add(new ProjectStep
                {
                    ProjectId = projectId,
                    StepNumber = 5,
                    StepName = "Đàm phán hợp đồng",
                    StatusId = 1, // InProgress
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
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
