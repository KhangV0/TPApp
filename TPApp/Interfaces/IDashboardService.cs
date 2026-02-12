using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Service interface for dashboard operations
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Get dashboard data for a specific user
        /// </summary>
        /// <param name="userId">User ID (string from Identity)</param>
        /// <returns>Dashboard view model with statistics and project list</returns>
        Task<UserDashboardVm> GetDashboardForUserAsync(string userId);
    }
}
