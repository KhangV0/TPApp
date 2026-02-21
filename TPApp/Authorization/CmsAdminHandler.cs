using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;

namespace TPApp.Authorization
{
    public class CmsAdminHandler : AuthorizationHandler<CmsAdminRequirement>
    {
        private readonly AppDbContext _context;

        public CmsAdminHandler(AppDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CmsAdminRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return;

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.IsAdmin == true)
            {
                context.Succeed(requirement);
            }
        }
    }
}
