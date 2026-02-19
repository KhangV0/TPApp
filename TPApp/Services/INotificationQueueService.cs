using TPApp.Enums;

namespace TPApp.Services
{
    /// <summary>
    /// Queues a Notification record (Status=Pending) so the background
    /// NotificationProcessor picks it up and sends email / SMS / SignalR push.
    /// </summary>
    public interface INotificationQueueService
    {
        /// <summary>Queue for int userId.</summary>
        Task QueueAsync(
            int userId,
            int? projectId,
            string title,
            string content,
            NotificationChannel channel = NotificationChannel.Email);

        /// <summary>Queue for string userId (Identity).</summary>
        Task QueueAsync(
            string userId,
            int? projectId,
            string title,
            string content,
            NotificationChannel channel = NotificationChannel.Email);
    }
}
