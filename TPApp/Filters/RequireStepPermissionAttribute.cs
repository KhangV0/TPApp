using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Filters
{
    /// <summary>
    /// Action filter to enforce step-based permissions
    /// Usage: [RequireStepPermission(stepNumber: 3, action: PermissionAction.View)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireStepPermissionAttribute : TypeFilterAttribute
    {
        public RequireStepPermissionAttribute(int stepNumber, PermissionAction action)
            : base(typeof(RequireStepPermissionFilter))
        {
            Arguments = new object[] { stepNumber, action };
        }

        private class RequireStepPermissionFilter : IAsyncActionFilter
        {
            private readonly int _stepNumber;
            private readonly PermissionAction _action;
            private readonly IPermissionService _permissionService;
            private readonly UserManager<ApplicationUser> _userManager;

            public RequireStepPermissionFilter(
                int stepNumber,
                PermissionAction action,
                IPermissionService permissionService,
                UserManager<ApplicationUser> userManager)
            {
                _stepNumber = stepNumber;
                _action = action;
                _permissionService = permissionService;
                _userManager = userManager;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                // Get current user
                var user = await _userManager.GetUserAsync(context.HttpContext.User);
                if (user == null)
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                    return;
                }

                // Get project ID from route
                var projectId = GetProjectIdFromContext(context);
                if (projectId == null)
                {
                    context.Result = new BadRequestObjectResult("Project ID is required");
                    return;
                }

                try
                {
                    // Check permission
                    await _permissionService.EnsureCanAsync(projectId.Value, _stepNumber, user.Id, _action);

                    // Permission granted, continue to action
                    await next();
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Permission denied, redirect to access denied page
                    context.HttpContext.Items["BlockedReason"] = ex.Message;
                    context.Result = new RedirectToActionResult("AccessDenied", "Project", new
                    {
                        projectId = projectId.Value,
                        stepNumber = _stepNumber,
                        reason = ex.Message
                    });
                }
            }

            private int? GetProjectIdFromContext(ActionExecutingContext context)
            {
                // Try to get from route values
                if (context.RouteData.Values.TryGetValue("id", out var idValue))
                {
                    if (int.TryParse(idValue?.ToString(), out var id))
                    {
                        return id;
                    }
                }

                // Try to get from action parameters
                if (context.ActionArguments.TryGetValue("id", out var paramId))
                {
                    if (paramId is int intId)
                    {
                        return intId;
                    }
                }

                if (context.ActionArguments.TryGetValue("projectId", out var paramProjectId))
                {
                    if (paramProjectId is int projectIdInt)
                    {
                        return projectIdInt;
                    }
                }

                return null;
            }
        }
    }
}
