using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TPApp.Interfaces;

namespace TPApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Admin Analytics Dashboard Controller.
    /// Executes exactly 1 snapshot read per request (scalable to 1M+ rows).
    /// All heavy analytics run in background jobs.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class AdminAnalyticsController : Controller
    {
        private readonly IAdminDashboardService _dashSvc;
        private readonly ILogger<AdminAnalyticsController> _logger;

        public AdminAnalyticsController(
            IAdminDashboardService dashSvc,
            ILogger<AdminAnalyticsController> logger)
        {
            _dashSvc = dashSvc;
            _logger  = logger;
        }

        // GET: /cms/AdminAnalytics
        // GET: /cms/AdminAnalytics?year=2025&month=6
        [HttpGet]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            try
            {
                var vm = await _dashSvc.GetAdminDashboardAsync(year, month);
                ViewData["Title"] = "Analytics Dashboard";
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdminAnalytics] Dashboard failed to load. Message: {Msg}", ex.Message);

                // In Development: surface the real error so you can debug quickly
                var env = HttpContext.RequestServices.GetService<IWebHostEnvironment>();
                if (env?.IsDevelopment() == true)
                {
                    TempData["Error"] = $"[DEV] {ex.GetType().Name}: {ex.Message}";
                    if (ex.InnerException != null)
                        TempData["Error"] += $" → {ex.InnerException.Message}";
                }
                else
                {
                    TempData["Error"] = "Không thể tải dữ liệu dashboard. Vui lòng thử lại sau.";
                }

                ViewData["Title"] = "Analytics Dashboard";
                return View(new TPApp.ViewModel.AdminDashboardVm());
            }
        }

        // POST: /cms/AdminAnalytics/RefreshSnapshot
        // Manual trigger for admins
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshSnapshot()
        {
            await _dashSvc.RebuildSnapshotAsync();
            await _dashSvc.RebuildMonthlyStatsAsync();
            TempData["Success"] = "Snapshot đã được cập nhật thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
