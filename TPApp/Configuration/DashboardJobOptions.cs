namespace TPApp.Configuration
{
    /// <summary>
    /// Options for the Dashboard background job.
    /// Bound from appsettings.json section "DashboardJob".
    ///
    /// Example appsettings.json:
    /// "DashboardJob": {
    ///   "SnapshotIntervalMinutes": 10,
    ///   "MonthlyRebuildIntervalMinutes": 1440
    /// }
    /// </summary>
    public class DashboardJobOptions
    {
        public const string SectionName = "DashboardJob";

        /// <summary>
        /// How often (in minutes) to rebuild the DashboardSnapshot row.
        /// Default: 10 minutes.
        /// </summary>
        public int SnapshotIntervalMinutes { get; set; } = 10;

        /// <summary>
        /// How often (in minutes) to rebuild the DashboardMonthlyStats for the current month.
        /// Default: 1440 minutes (24 hours).
        /// </summary>
        public int MonthlyRebuildIntervalMinutes { get; set; } = 1440;

        // Derived helpers
        public TimeSpan SnapshotInterval       => TimeSpan.FromMinutes(SnapshotIntervalMinutes);
        public TimeSpan MonthlyRebuildInterval => TimeSpan.FromMinutes(MonthlyRebuildIntervalMinutes);
    }
}
