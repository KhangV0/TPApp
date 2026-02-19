using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TPApp.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Client calls this after connecting to join their personal group.
        /// Group key = userId so server can push to specific user.
        /// </summary>
        public async Task JoinUserGroup(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
