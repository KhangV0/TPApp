using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TPApp.Data;
using TPApp.Enums;

namespace TPApp.Controllers
{
    [Authorize]
    [Route("api/notifications")]
    [ApiController]
    public class NotificationApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationApiController(AppDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // GET /api/notifications/latest
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] int take = 10)
        {
            var userId = CurrentUserId;
            var items = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == (int)NotificationStatus.Success)
                .OrderByDescending(n => n.CreatedDate)
                .Take(take)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Content,
                    n.IsRead,
                    n.CreatedDate,
                    n.ProjectId,
                    n.Channel
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET /api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == CurrentUserId
                              && n.Status == (int)NotificationStatus.Success
                              && !n.IsRead);
            return Ok(new { count });
        }

        // POST /api/notifications/mark-read/{id}
        [HttpPost("mark-read/{id:int}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var n = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId);
            if (n == null) return NotFound();

            n.IsRead   = true;
            n.ReadDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST /api/notifications/mark-all-read
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == CurrentUserId
                         && n.Status == (int)NotificationStatus.Success
                         && !n.IsRead)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var n in unread)
            {
                n.IsRead   = true;
                n.ReadDate = now;
            }

            await _context.SaveChangesAsync();
            return Ok(new { marked = unread.Count });
        }
    }
}
