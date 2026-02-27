using Microsoft.Extensions.Options;
using TPApp.Configuration;
using TPApp.Interfaces;

namespace TPApp.BackgroundServices
{
    /// <summary>
    /// Hosted background service for Dashboard pre-computation.
    ///
    /// Intervals are fully configurable from appsettings.json:
    ///   "DashboardJob": {
    ///     "SnapshotIntervalMinutes": 10,
    ///     "MonthlyRebuildIntervalMinutes": 1440
    ///   }
    ///
    /// Uses IServiceScope per tick to safely resolve scoped services.
    /// All exceptions are caught and logged — never crashes the host.
    /// </summary>
    public class DashboardBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DashboardBackgroundService> _logger;
        private readonly DashboardJobOptions _options;

        private DateTime _lastMonthlyRebuild = DateTime.MinValue;

        public DashboardBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<DashboardBackgroundService> logger,
            IOptions<DashboardJobOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
            _options      = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "[DashboardBg] Service started. Snapshot every {S} min, Monthly every {M} min.",
                _options.SnapshotIntervalMinutes,
                _options.MonthlyRebuildIntervalMinutes);

            // Warm-up rebuild on startup
            await RunSnapshotRebuildAsync();
            await RunMonthlyRebuildAsync();
            _lastMonthlyRebuild = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.SnapshotInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                await RunSnapshotRebuildAsync();

                if (DateTime.UtcNow - _lastMonthlyRebuild >= _options.MonthlyRebuildInterval)
                {
                    await RunMonthlyRebuildAsync();
                    _lastMonthlyRebuild = DateTime.UtcNow;
                }
            }

            _logger.LogInformation("[DashboardBg] Service stopping.");
        }

        private async Task RunSnapshotRebuildAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc         = scope.ServiceProvider.GetRequiredService<IAdminDashboardService>();
                await svc.RebuildSnapshotAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DashboardBg] Snapshot rebuild failed.");
            }
        }

        private async Task RunMonthlyRebuildAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc         = scope.ServiceProvider.GetRequiredService<IAdminDashboardService>();
                await svc.RebuildMonthlyStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DashboardBg] Monthly stats rebuild failed.");
            }
        }
    }
}
