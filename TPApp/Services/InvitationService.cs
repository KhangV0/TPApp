using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    /// <summary>
    /// Service for managing RFQ invitations with strict guards
    /// </summary>
    public class InvitationService : IInvitationService
    {
        private readonly AppDbContext _context;
        private readonly IESignGateway _eSignGateway;

        public InvitationService(AppDbContext context, IESignGateway eSignGateway)
        {
            _context = context;
            _eSignGateway = eSignGateway;
        }

        public async Task<RFQInvitation> CreateInvitationAsync(int projectId, int rfqId, int sellerId)
        {
            // Check if invitation already exists
            var existing = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && 
                                         i.SellerId == sellerId && 
                                         i.IsActive);

            if (existing != null)
            {
                return existing; // Return existing invitation
            }

            var invitation = new RFQInvitation
            {
                ProjectId = projectId,
                RFQId = rfqId,
                SellerId = sellerId,
                InvitedDate = DateTime.Now,
                StatusId = (int)RFQInvitationStatus.Invited,
                IsActive = true
            };

            _context.RFQInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return invitation;
        }

        public async Task<bool> CanAcceptInvitationAsync(int invitationId, int userId)
        {
            var invitation = await _context.RFQInvitations
                .Include(i => i.RFQRequest)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null) return false;

            // Guard 1: User must be the invited seller
            if (invitation.SellerId != userId) return false;

            // Guard 2: Invitation must be active
            if (!invitation.IsActive) return false;

            // Guard 3: Status must be Invited or Viewed
            if (invitation.StatusId != (int)RFQInvitationStatus.Invited && 
                invitation.StatusId != (int)RFQInvitationStatus.Viewed)
            {
                return false;
            }

            // Guard 4: RFQ must not be expired
            if (invitation.RFQRequest?.HanChotNopHoSo < DateTime.Now)
            {
                return false;
            }

            // Guard 5: NDA must be signed
            var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(invitation.ProjectId, userId);
            if (!ndaSigned)
            {
                return false;
            }

            return true;
        }

        public async Task AcceptInvitationAsync(int invitationId, int userId)
        {
            if (!await CanAcceptInvitationAsync(invitationId, userId))
            {
                throw new InvalidOperationException("Cannot accept invitation. Check NDA signature, invitation status, and deadline.");
            }

            var invitation = await _context.RFQInvitations.FindAsync(invitationId);
            if (invitation == null)
            {
                throw new InvalidOperationException("Invitation not found");
            }

            // Update invitation status
            invitation.StatusId = (int)RFQInvitationStatus.Accepted;
            invitation.ResponseDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // IMPORTANT: Do NOT set Project.SelectedSellerId here
            // Seller is only assigned when proposal is selected
        }

        public async Task DeclineInvitationAsync(int invitationId, int userId, string? reason = null)
        {
            var invitation = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.Id == invitationId && i.SellerId == userId);

            if (invitation == null)
            {
                throw new InvalidOperationException("Invitation not found or you are not authorized");
            }

            invitation.StatusId = (int)RFQInvitationStatus.Declined;
            invitation.ResponseDate = DateTime.Now;
            invitation.Notes = reason;
            invitation.IsActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasValidInvitationAsync(int projectId, int sellerId)
        {
            return await _context.RFQInvitations
                .AnyAsync(i => i.ProjectId == projectId && 
                              i.SellerId == sellerId && 
                              i.IsActive && 
                              i.StatusId != (int)RFQInvitationStatus.Declined);
        }

        public async Task<RFQInvitation?> GetInvitationByIdAsync(int invitationId)
        {
            return await _context.RFQInvitations
                .Include(i => i.Project)
                .Include(i => i.RFQRequest)
                .FirstOrDefaultAsync(i => i.Id == invitationId);
        }

        public async Task<List<RFQInvitation>> GetSellerInvitationsAsync(int sellerId)
        {
            return await _context.RFQInvitations
                .Include(i => i.Project)
                .Include(i => i.RFQRequest)
                .Where(i => i.SellerId == sellerId && i.IsActive)
                .OrderByDescending(i => i.InvitedDate)
                .ToListAsync();
        }
    }
}
