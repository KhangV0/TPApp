namespace TPApp.Interfaces
{
    /// <summary>
    /// Service for managing seller selection and proposal approval
    /// </summary>
    public interface ISelectionService
    {
        /// <summary>
        /// Check if buyer can select this seller's proposal
        /// </summary>
        Task<bool> CanSelectSellerAsync(int projectId, int proposalId, int buyerUserId);

        /// <summary>
        /// Select a seller's proposal, reject others, and transition workflow
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when selection cannot be completed</exception>
        Task SelectSellerAsync(int projectId, int proposalId, int buyerUserId);

        /// <summary>
        /// Get selected seller ID for project (if any)
        /// </summary>
        Task<int?> GetSelectedSellerIdAsync(int projectId);

        /// <summary>
        /// Check if project has a selected seller
        /// </summary>
        Task<bool> HasSelectedSellerAsync(int projectId);
    }
}
