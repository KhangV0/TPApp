using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.ViewComponents
{
    public class RFQInvitationBadgeViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RFQInvitationBadgeViewComponent(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Content(string.Empty);
            }

            // Get current user ID
            var userIdString = _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Content(string.Empty);
            }

            // Check if user is a seller (has NhaCungUng record with this UserId)
            var isSeller = await _context.NhaCungUngs.AnyAsync(n => n.UserId == userId);
            if (!isSeller)
            {
                return Content(string.Empty);
            }

            // Count pending invitations (StatusId = 0 or 1, IsActive = true)
            var pendingCount = await _context.RFQInvitations
                .Where(i => i.SellerId == userId && 
                           i.IsActive && 
                           (i.StatusId == 0 || i.StatusId == 1)) // Invited or Viewed
                .CountAsync();

            if (pendingCount == 0)
            {
                return Content(string.Empty);
            }

            // Return view with count
            return View(pendingCount);
        }
    }
}
