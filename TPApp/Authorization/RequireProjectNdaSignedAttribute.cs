using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Authorization
{
    /// <summary>
    /// Authorization attribute to verify that a user has signed the NDA for a project
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireProjectNdaSignedAttribute : TypeFilterAttribute
    {
        public RequireProjectNdaSignedAttribute() : base(typeof(ProjectNdaSignedFilter))
        {
        }
    }

    public class ProjectNdaSignedFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IESignGateway _eSignGateway;

        public ProjectNdaSignedFilter(
            AppDbContext context, 
            UserManager<ApplicationUser> userManager,
            IESignGateway eSignGateway)
        {
            _context = context;
            _userManager = userManager;
            _eSignGateway = eSignGateway;
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

            // If user is Buyer (Role=1), they don't need to sign NDA (they created it)
            if (member.Role == 1)
            {
                await next();
                return;
            }

            // For Sellers and Consultants, check NDA signature
            var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(projectId.Value, userId);

            if (!ndaSigned)
            {
                // NDA not signed - redirect to NDA signing page
                context.Result = new RedirectToActionResult("SignNda", "Project", 
                    new { projectId = projectId });
                return;
            }

            // NDA signed - allow access
            await next();
        }
    }
}
