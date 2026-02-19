using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /Notifications
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == 1) // 1 = Success
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return View(notifications);
        }

        // POST /Notifications/MarkRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = _userManager.GetUserId(User);
            var n = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n != null)
            {
                n.IsRead = true;
                n.ReadDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST /Notifications/MarkAllRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _userManager.GetUserId(User);
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == 1 && !n.IsRead) // 1 = Success
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadDate = now;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
