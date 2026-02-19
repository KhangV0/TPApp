using TPApp.Entities;

namespace TPApp.Interfaces
{
    public interface INotificationProcessor
    {
        Task ProcessAsync(Notification notification, CancellationToken cancellationToken = default);
    }
}
