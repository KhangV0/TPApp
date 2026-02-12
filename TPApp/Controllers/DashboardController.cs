using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    /// <summary>
    /// Dashboard controller for displaying user project overview
    /// Requires authentication
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Display user dashboard with project statistics and workflow progress
        /// </summary>
        public async Task<IActionResult> Index([FromServices] TPApp.Data.AppDbContext context)
        {
            try
            {
                // Get current user ID - try multiple approaches for compatibility
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Fallback: try to get from UserName claim and lookup in database
                if (string.IsNullOrEmpty(userId))
                {
                    var userName = User.Identity?.Name;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var user = context.Users.FirstOrDefault(u => u.UserName == userName);
                        if (user != null)
                        {
                            userId = user.Id.ToString();
                        }
                    }
                }
                
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get dashboard data from service
                var dashboardData = await _dashboardService.GetDashboardForUserAsync(userId);

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                // Log error (in production, use proper logging)
                Console.WriteLine($"Dashboard error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Show error details in development
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
