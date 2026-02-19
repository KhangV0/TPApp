using TPApp.Interfaces;

namespace TPApp.Services
{
    /// <summary>
    /// No-op SMS sender used when Twilio is not configured or not available.
    /// Logs the message instead of sending it.
    /// </summary>
    public class StubSmsSender : ISmsSender
    {
        private readonly ILogger<StubSmsSender> _logger;

        public StubSmsSender(ILogger<StubSmsSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string to, string message)
        {
            _logger.LogInformation("[SMS-STUB] Would send to {To}: {Message}", to, message);
            return Task.CompletedTask;
        }
    }
}
