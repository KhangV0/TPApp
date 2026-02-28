using Microsoft.AspNetCore.Identity;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;

namespace TPApp.Services
{
    public class NotificationQueueService : INotificationQueueService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationQueueService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task QueueAsync(
            int userId,
            int? projectId,
            string title,
            string content,
            NotificationChannel channel = NotificationChannel.Email,
            string? url = null)
        {
            await QueueAsync(userId.ToString(), projectId, title, content, channel, url);
        }

        public async Task QueueAsync(
            string userId,
            int? projectId,
            string title,
            string content,
            NotificationChannel channel = NotificationChannel.Email,
            string? url = null)
        {
            // Resolve email/phone as Target
            string? target = null;
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                target = channel == NotificationChannel.Sms
                    ? user.PhoneNumber
                    : user.Email;
            }

            var notification = new Notification
            {
                UserId      = userId,
                Target      = target,
                Title       = title,
                Content     = content,
                ProjectId   = projectId,
                Channel     = (int)channel,
                Status      = (int)NotificationStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                Url         = url
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
