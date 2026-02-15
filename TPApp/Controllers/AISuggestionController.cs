using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TPApp.Application.Services;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    /// <summary>
    /// API controller for AI-powered supplier suggestions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AISuggestionController : ControllerBase
    {
        private readonly IAISupplierMatchingService _matchingService;
        private readonly ILogger<AISuggestionController> _logger;

        private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitCache = new();
        private const int MAX_REQUESTS_PER_MINUTE = 10;
        private const int RATE_LIMIT_WINDOW_SECONDS = 60;

        public AISuggestionController(
            IAISupplierMatchingService matchingService,
            ILogger<AISuggestionController> logger)
        {
            _matchingService = matchingService ?? throw new ArgumentNullException(nameof(matchingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Suggests suppliers based on AI semantic matching.
        /// </summary>
        /// <param name="request">The suggestion request containing the query text</param>
        /// <returns>List of matched suppliers with scores and top products</returns>
        [HttpPost("suggest")]
        public async Task<IActionResult> Suggest([FromBody] SuggestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (!CheckRateLimit(clientIp))
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IP}", clientIp);
                return StatusCode(429, new { error = "Too many requests. Please try again later." });
            }

            try
            {
                var sanitizedInput = SanitizeInput(request.Input);

                _logger.LogInformation("AI suggestion request from {IP}: {Query}", clientIp, sanitizedInput);

                var results = await _matchingService.FindMatchingSuppliersAsync(sanitizedInput);

                var response = new SuggestResponse
                {
                    Suppliers = results
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for AI suggestion");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI suggestion request");
                return StatusCode(500, new { error = "An error occurred processing your request" });
            }
        }

        private bool CheckRateLimit(string clientIp)
        {
            var now = DateTime.UtcNow;

            _rateLimitCache.AddOrUpdate(
                clientIp,
                _ => new RateLimitInfo { Count = 1, WindowStart = now },
                (_, existing) =>
                {
                    if ((now - existing.WindowStart).TotalSeconds > RATE_LIMIT_WINDOW_SECONDS)
                    {
                        return new RateLimitInfo { Count = 1, WindowStart = now };
                    }
                    else
                    {
                        existing.Count++;
                        return existing;
                    }
                });

            CleanupOldEntries();

            return _rateLimitCache.TryGetValue(clientIp, out var info) && info.Count <= MAX_REQUESTS_PER_MINUTE;
        }

        private void CleanupOldEntries()
        {
            var now = DateTime.UtcNow;
            var keysToRemove = _rateLimitCache
                .Where(kvp => (now - kvp.Value.WindowStart).TotalSeconds > RATE_LIMIT_WINDOW_SECONDS * 2)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _rateLimitCache.TryRemove(key, out _);
            }
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var sanitized = input.Trim();

            var dangerousPatterns = new[] { "<script", "javascript:", "onerror=", "onclick=" };
            foreach (var pattern in dangerousPatterns)
            {
                if (sanitized.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Input contains potentially dangerous content");
                }
            }

            return sanitized;
        }

        private class RateLimitInfo
        {
            public int Count { get; set; }
            public DateTime WindowStart { get; set; }
        }
    }
}
