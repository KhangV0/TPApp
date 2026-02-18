using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Service for managing project members (consultants, sellers, buyers)
    /// </summary>
    public interface IProjectMemberService
    {
        /// <summary>
        /// Get all active members of a project
        /// </summary>
        Task<List<ProjectMemberDto>> GetMembersAsync(int projectId);

        /// <summary>
        /// Add a consultant to the project
        /// Guards: Only buyer can add, cannot add duplicates
        /// </summary>
        Task AddConsultantAsync(int projectId, int userId, int currentUserId);

        /// <summary>
        /// Remove a member from the project (soft delete)
        /// Guards: Only buyer can remove, cannot remove buyer role
        /// </summary>
        Task RemoveMemberAsync(int projectId, int userId, int currentUserId);

        /// <summary>
        /// Check if user is buyer of the project
        /// </summary>
        Task<bool> IsBuyerAsync(int projectId, int currentUserId);

        /// <summary>
        /// Get users who are not members of the project (for dropdown)
        /// </summary>
        Task<List<UserSelectDto>> GetAvailableConsultantsAsync(int projectId);
    }
}
