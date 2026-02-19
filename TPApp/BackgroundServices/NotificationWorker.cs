using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TPApp.Configuration;
using TPApp.Data;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.BackgroundServices
{
    /// <summary>
    /// Long-running background worker that polls the Notifications table
    /// and processes pending notifications via INotificationProcessor.
    /// </summary>
    public class NotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationWorker> _logger;

        private const int DefaultBatchSize        = 20;
        private const int DefaultIntervalSeconds  = 5;

        public NotificationWorker(IServiceProvider serviceProvider, ILogger<NotificationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger          = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Unhandled error in NotificationWorker loop.");
                }

                // Read interval from SYS_PARAMETERS each cycle (allows runtime config change)
                int intervalSeconds = DefaultIntervalSeconds;
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var paramSvc = scope.ServiceProvider.GetRequiredService<ISystemParameterService>();
                    intervalSeconds = await paramSvc.GetIntAsync(ParameterKeys.NotificationIntervalSeconds, DefaultIntervalSeconds);
                }
                catch { /* use default */ }

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }

            _logger.LogInformation("NotificationWorker stopped.");
        }

        private async Task ProcessBatchAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context   = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
            var paramSvc  = scope.ServiceProvider.GetRequiredService<ISystemParameterService>();

            int batchSize = await paramSvc.GetIntAsync(ParameterKeys.NotificationBatchSize, DefaultBatchSize);

            var pending = await context.Notifications
                .Where(n => n.Status == (int)NotificationStatus.Pending)
                .OrderBy(n => n.CreatedDate)
                .Take(batchSize)
                .ToListAsync(stoppingToken);

            if (pending.Count == 0) return;

            _logger.LogInformation("NotificationWorker: processing {Count} pending notification(s).", pending.Count);

            foreach (var notification in pending)
            {
                if (stoppingToken.IsCancellationRequested) break;

                await processor.ProcessAsync(notification, stoppingToken);
            }
        }
    }
}
