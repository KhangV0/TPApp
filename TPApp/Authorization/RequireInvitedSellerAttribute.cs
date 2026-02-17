using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Authorization
{
    /// <summary>
    /// Authorization attribute to verify that a seller has been invited to a project via RFQ
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireInvitedSellerAttribute : TypeFilterAttribute
    {
        public RequireInvitedSellerAttribute() : base(typeof(InvitedSellerFilter))
        {
        }
    }

    public class InvitedSellerFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvitedSellerFilter(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get current user ID
            var userIdString = _userManager.GetUserId(context.HttpContext.User);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get project ID from route or query string
            int? projectId = null;
            if (context.ActionArguments.TryGetValue("duAnId", out var duAnIdObj))
            {
                projectId = duAnIdObj as int?;
            }
            else if (context.ActionArguments.TryGetValue("projectId", out var projectIdObj))
            {
                projectId = projectIdObj as int?;
            }
            else if (context.HttpContext.Request.Query.TryGetValue("duAnId", out var duAnIdQuery))
            {
                if (int.TryParse(duAnIdQuery, out int parsedId))
                {
                    projectId = parsedId;
                }
            }
            else if (context.HttpContext.Request.Query.TryGetValue("projectId", out var projectIdQuery))
            {
                if (int.TryParse(projectIdQuery, out int parsedId))
                {
                    projectId = parsedId;
                }
            }

            if (projectId == null)
            {
                context.Result = new BadRequestObjectResult("Project ID is required");
                return;
            }

            // Check if user is a project member
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (member == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // If user is Buyer (Role=1), allow access (they own the project)
            if (member.Role == 1)
            {
                await next();
                return;
            }

            // For Sellers (Role=2) and Consultants (Role=3), check invitation
            var invitation = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && 
                                         i.SellerId == userId && 
                                         i.IsActive);

            if (invitation == null)
            {
                // No invitation found - redirect to access denied
                context.Result = new RedirectToActionResult("AccessDenied", "Home", 
                    new { reason = "not_invited", projectId = projectId });
                return;
            }

            // Invitation exists - allow access
            await next();
        }
    }
}
