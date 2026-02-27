using Microsoft.AspNetCore.Mvc;
using TPApp.Interfaces;

namespace TPApp.Controllers.Api
{
    /// <summary>
    /// GET /api/home/analytics
    /// Returns cached JSON payload for the Home Analytics section.
    /// Used by client-side JS when lazy-loading advanced analytics.
    /// </summary>
    [ApiController]
    [Route("api/home")]
    public class HomeAnalyticsApiController : ControllerBase
    {
        private readonly IHomeAnalyticsService _svc;

        public HomeAnalyticsApiController(IHomeAnalyticsService svc)
        {
            _svc = svc;
        }

        /// <summary>Returns full analytics JSON (5-min cached).</summary>
        [HttpGet("analytics")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetAnalytics()
        {
            var data = await _svc.GetHomeAnalyticsJsonAsync();
            return Ok(data);
        }
    }
}
