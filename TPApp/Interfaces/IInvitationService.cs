using TPApp.Entities;
using TPApp.Enums;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Service for managing RFQ invitations and seller acceptance workflow
    /// </summary>
    public interface IInvitationService
    {
        /// <summary>
        /// Create a new invitation for a seller
        /// </summary>
        Task<RFQInvitation> CreateInvitationAsync(int projectId, int rfqId, int sellerId);

        /// <summary>
        /// Check if seller can accept invitation (NDA signed, not expired, valid status)
        /// </summary>
        Task<bool> CanAcceptInvitationAsync(int invitationId, int userId);

        /// <summary>
        /// Accept invitation and update status to Accepted
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when invitation cannot be accepted</exception>
        Task AcceptInvitationAsync(int invitationId, int userId);

        /// <summary>
        /// Decline invitation with optional reason
        /// </summary>
        Task DeclineInvitationAsync(int invitationId, int userId, string? reason = null);

        /// <summary>
        /// Check if seller has a valid (active, not declined) invitation for project
        /// </summary>
        Task<bool> HasValidInvitationAsync(int projectId, int sellerId);

        /// <summary>
        /// Get invitation by ID
        /// </summary>
        Task<RFQInvitation?> GetInvitationByIdAsync(int invitationId);

        /// <summary>
        /// Get all invitations for a seller
        /// </summary>
        Task<List<RFQInvitation>> GetSellerInvitationsAsync(int sellerId);
    }
}
