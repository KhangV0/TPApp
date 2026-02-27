using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Admin-level analytics dashboard service.
    /// Fast path: reads from DashboardSnapshot (1 row query).
    /// Heavy analytics (GroupBy etc.) executed only in background job.
    /// </summary>
    public interface IAdminDashboardService
    {
        // ── Fast path ──────────────────────────────────────────────
        /// <summary>
        /// Build the full AdminDashboardVm.
        /// Snapshot comes from cached DB row (1 query).
        /// Monthly stats limited to last 12 months.
        /// Analytics (Top-5s) come from pre-computed data or lightweight queries.
        /// </summary>
        Task<AdminDashboardVm> GetAdminDashboardAsync(int? filterYear = null, int? filterMonth = null);

        // ── Background rebuild methods (called by DashboardBackgroundService) ──
        Task RebuildSnapshotAsync();
        Task RebuildMonthlyStatsAsync();
    }
}
