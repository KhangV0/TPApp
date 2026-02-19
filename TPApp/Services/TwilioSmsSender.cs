// TwilioSmsSender is disabled — Twilio package removed.
// To re-enable: add <PackageReference Include="Twilio" Version="7.4.0" /> to csproj
// and register in Program.cs: builder.Services.AddScoped<ISmsSender, TwilioSmsSender>();

/*
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using TPApp.Configuration;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly ISystemParameterService _params;
        private readonly ILogger<TwilioSmsSender> _logger;

        public TwilioSmsSender(ISystemParameterService @params, ILogger<TwilioSmsSender> logger)
        {
            _params = @params;
            _logger = logger;
        }

        public async Task SendAsync(string to, string message)
        {
            var accountId  = await _params.GetRequiredAsync(ParameterKeys.SmsAccountId);
            var authToken  = await _params.GetRequiredAsync(ParameterKeys.SmsAuthToken);
            var fromNumber = await _params.GetRequiredAsync(ParameterKeys.SmsFromNumber);
            var maxRetry   = await _params.GetIntAsync(ParameterKeys.SmsMaxRetry, 3);
            var retryDelay = await _params.GetIntAsync(ParameterKeys.SmsRetryDelaySeconds, 5);

            TwilioClient.Init(accountId, authToken);

            Exception? lastEx = null;
            for (int attempt = 1; attempt <= maxRetry; attempt++)
            {
                try
                {
                    var msg = await MessageResource.CreateAsync(
                        body: message,
                        from: new Twilio.Types.PhoneNumber(fromNumber),
                        to:   new Twilio.Types.PhoneNumber(to)
                    );
                    return;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    if (attempt < maxRetry)
                        await Task.Delay(TimeSpan.FromSeconds(retryDelay));
                }
            }
            throw new Exception($"SMS failed after {maxRetry} attempts: {lastEx?.Message}", lastEx);
        }
    }
}
*/
