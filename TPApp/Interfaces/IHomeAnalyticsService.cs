using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Home Analytics Service — read-only consumer of Admin Dashboard snapshot.
    /// Inherits data from DashboardSnapshot + DashboardMonthlyStats via IMemoryCache.
    /// </summary>
    public interface IHomeAnalyticsService
    {
        /// <summary>
        /// Returns the full analytics ViewModel for the home page.
        /// Cached for 5 minutes. Max 2 DB queries per cache miss.
        /// </summary>
        Task<HomeAnalyticsVm> GetHomeAnalyticsAsync();

        /// <summary>
        /// Returns the serialized JSON payload for the GET /api/home/analytics endpoint.
        /// </summary>
        Task<object> GetHomeAnalyticsJsonAsync();
    }
}
