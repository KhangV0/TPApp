using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Route("api/entity")]
    public class EntityActionController : Controller
    {
        private readonly IEntityActionService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public EntityActionController(IEntityActionService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        private async Task<string?> GetUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id.ToString();
        }

        // ── GET /api/entity/{entityType}/{entityId}/summary ─────────────
        [HttpGet("{entityType}/{entityId:int}/summary")]
        public async Task<IActionResult> Summary(string entityType, int entityId)
        {
            if (!EntityTypes.IsValid(entityType))
                return BadRequest(new { error = "Loại entity không hợp lệ." });

            var userId = await GetUserIdAsync();
            var summary = await _service.GetSummaryAsync(entityType, entityId, userId);
            return Ok(summary);
        }

        // ── POST /api/entity/{entityType}/{entityId}/rating/save ────────
        [HttpPost("{entityType}/{entityId:int}/rating/save")]
        public async Task<IActionResult> SaveRating(
            string entityType, int entityId, [FromBody] SaveRatingRequest request)
        {
            if (!EntityTypes.IsValid(entityType))
                return BadRequest(new { error = "Loại entity không hợp lệ." });

            var userId = await GetUserIdAsync();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Vui lòng đăng nhập." });

            var result = await _service.SaveRatingAsync(entityType, entityId, userId, request);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result);
        }

        // ── POST /api/entity/{entityType}/{entityId}/view/increase ──────
        [HttpPost("{entityType}/{entityId:int}/view/increase")]
        public async Task<IActionResult> IncreaseView(string entityType, int entityId)
        {
            if (!EntityTypes.IsValid(entityType))
                return BadRequest(new { error = "Loại entity không hợp lệ." });

            // Anti-spam: cookie check
            var cookieKey = $"view_{entityType}_{entityId}";
            if (Request.Cookies.ContainsKey(cookieKey))
                return Ok(new { skipped = true });

            var userId = await GetUserIdAsync();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _service.IncreaseViewAsync(entityType, entityId, userId, ip);

            // Set cookie: expire in 5 minutes
            Response.Cookies.Append(cookieKey, "1", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });

            return Ok(new { success = true });
        }

        // ── GET /api/entity/{entityType}/{entityId}/ratings ─────────────
        [HttpGet("{entityType}/{entityId:int}/ratings")]
        public async Task<IActionResult> GetRatings(string entityType, int entityId, [FromQuery] int take = 20)
        {
            if (!EntityTypes.IsValid(entityType))
                return BadRequest(new { error = "Loại entity không hợp lệ." });

            var ratings = await _service.GetRatingsAsync(entityType, entityId, take);
            return Ok(new { ratings });
        }
    }
}
