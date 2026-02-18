using TPApp.Enums;
using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Service for checking and enforcing step-based permissions
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Get permissions for a user on a specific project step
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="stepNumber">Step number (1-14)</param>
        /// <param name="userId">User ID</param>
        /// <returns>Permission result with flags and blocked reason</returns>
        Task<StepPermissionResult> GetStepPermissionsAsync(int projectId, int stepNumber, int userId);

        /// <summary>
        /// Ensure user has permission for specific action, throw exception if denied
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="stepNumber">Step number (1-14)</param>
        /// <param name="userId">User ID</param>
        /// <param name="action">Required action</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when permission is denied</exception>
        Task EnsureCanAsync(int projectId, int stepNumber, int userId, PermissionAction action);

        /// <summary>
        /// Get user's role type for a specific project
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="projectId">Project ID</param>
        /// <returns>User role type</returns>
        Task<UserRoleType> GetUserRoleAsync(int userId, int projectId);

        /// <summary>
        /// Check if user has signed NDA for project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="roleType">User role type</param>
        /// <returns>True if NDA is signed</returns>
        Task<bool> HasSignedNdaAsync(int projectId, int userId, UserRoleType roleType);
    }
}
