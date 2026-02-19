using Microsoft.AspNetCore.SignalR;
using TPApp.Configuration;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Hubs;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class NotificationProcessor : INotificationProcessor
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ISystemParameterService _params;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationProcessor> _logger;

        public NotificationProcessor(
            AppDbContext context,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ISystemParameterService @params,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationProcessor> logger)
        {
            _context    = context;
            _emailSender = emailSender;
            _smsSender   = smsSender;
            _params      = @params;
            _hubContext  = hubContext;
            _logger      = logger;
        }

        public async Task ProcessAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            var channel = (NotificationChannel)notification.Channel;

            // ── Channel enable/disable guard ─────────────────────────────────
            bool channelEnabled = channel switch
            {
                NotificationChannel.Email => await _params.GetBoolAsync(ParameterKeys.NotificationEnableEmail, true),
                NotificationChannel.Sms   => await _params.GetBoolAsync(ParameterKeys.NotificationEnableSms, true),
                _                         => false
            };

            if (!channelEnabled)
            {
                _logger.LogInformation(
                    "[Notification #{Id}] Channel {Channel} is disabled — skipping send, marking delivered for bell.",
                    notification.Id, channel);

                // Still mark as Success so the bell picks it up
                notification.Status   = (int)NotificationStatus.Success;
                notification.SentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                // Push realtime bell via SignalR
                if (!string.IsNullOrEmpty(notification.UserId))
                {
                    await _hubContext.Clients
                        .Group(notification.UserId)
                        .SendAsync("ReceiveNotification", new
                        {
                            id          = notification.Id,
                            title       = notification.Title,
                            content     = notification.Content,
                            createdDate = notification.CreatedDate,
                            projectId   = notification.ProjectId
                        }, cancellationToken);
                }
                return;
            }

            // Determine max retry per channel
            int maxRetry = channel switch
            {
                NotificationChannel.Email => await _params.GetIntAsync(ParameterKeys.SmtpMaxRetry, 3),
                NotificationChannel.Sms   => await _params.GetIntAsync(ParameterKeys.SmsMaxRetry, 3),
                _                         => 3
            };

            notification.LastTriedDate = DateTime.UtcNow;

            try
            {
                switch (channel)
                {
                    case NotificationChannel.Email:
                        if (string.IsNullOrEmpty(notification.Target))
                            throw new InvalidOperationException("Email target (To address) is empty.");
                        await _emailSender.SendAsync(
                            notification.Target,
                            notification.Title ?? "(no subject)",
                            notification.Content ?? "");
                        break;

                    case NotificationChannel.Sms:
                        if (string.IsNullOrEmpty(notification.Target))
                            throw new InvalidOperationException("SMS target (phone number) is empty.");
                        await _smsSender.SendAsync(
                            notification.Target,
                            notification.Content ?? "");
                        break;

                    default:
                        throw new NotSupportedException($"Channel {notification.Channel} is not supported.");
                }

                // ── Success ──────────────────────────────────────────────────
                notification.Status    = (int)NotificationStatus.Success;
                notification.SentDate  = DateTime.UtcNow;
                notification.LastError = null;

                _logger.LogInformation(
                    "[Notification #{Id}] Sent via {Channel} to {Target}",
                    notification.Id, channel, notification.Target);

                // ── Push realtime bell via SignalR ────────────────────────────
                if (!string.IsNullOrEmpty(notification.UserId))
                {
                    await _hubContext.Clients
                        .Group(notification.UserId)
                        .SendAsync("ReceiveNotification", new
                        {
                            id          = notification.Id,
                            title       = notification.Title,
                            content     = notification.Content,
                            createdDate = notification.CreatedDate,
                            projectId   = notification.ProjectId
                        }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // ── Failure ──────────────────────────────────────────────────
                notification.RetryCount++;
                notification.LastError = ex.Message.Length > 1000
                    ? ex.Message[..1000]
                    : ex.Message;

                notification.Status = notification.RetryCount < maxRetry
                    ? (int)NotificationStatus.Pending   // will retry
                    : (int)NotificationStatus.Failed;   // exhausted retries

                _logger.LogWarning(
                    "[Notification #{Id}] Failed (attempt {Retry}/{Max}): {Error}",
                    notification.Id, notification.RetryCount, maxRetry, ex.Message);
            }
            finally
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
