namespace TPApp.Enums
{
    /// <summary>
    /// Status of RFQ invitations sent to sellers
    /// </summary>
    public enum RFQInvitationStatus
    {
        /// <summary>
        /// Invitation sent, not yet viewed by seller
        /// </summary>
        Invited = 0,

        /// <summary>
        /// Seller has viewed the invitation
        /// </summary>
        Viewed = 1,

        /// <summary>
        /// Seller accepted the invitation
        /// </summary>
        Accepted = 2,

        /// <summary>
        /// Seller submitted a proposal
        /// </summary>
        ProposalSubmitted = 3,

        /// <summary>
        /// Seller declined the invitation
        /// </summary>
        Declined = 4,

        /// <summary>
        /// Invitation expired (past deadline)
        /// </summary>
        Expired = 5
    }
}
