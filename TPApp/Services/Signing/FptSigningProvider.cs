using TPApp.Interfaces;

namespace TPApp.Services.Signing
{
    /// <summary>Skeleton adapter for FPT CA remote signing.</summary>
    public class FptSigningProvider : ISigningProvider
    {
        private readonly ISystemParameterService _sysParams;
        private readonly ILogger<FptSigningProvider> _logger;

        public string ProviderName => "FPT";

        public FptSigningProvider(ISystemParameterService sysParams, ILogger<FptSigningProvider> logger)
        {
            _sysParams = sysParams;
            _logger    = logger;
        }

        public async Task<string> CreateSigningRequestAsync(byte[] pdfBytes, SignerInfo signer, string callbackUrl)
        {
            var apiBase = await _sysParams.GetAsync("SIGNING_FPT_API_BASE");
            var apiKey  = await _sysParams.GetAsync("SIGNING_FPT_API_KEY");

            if (string.IsNullOrEmpty(apiBase) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("FPT signing not configured. Returning stub RequestRef.");
                return $"FPT-STUB-{Guid.NewGuid():N}";
            }

            // TODO: Implement real FPT eSign API call
            _logger.LogInformation("FPT CreateSigningRequest → stub mode.");
            return $"FPT-{Guid.NewGuid():N}";
        }

        public async Task<SignedResult?> GetSignedDocumentAsync(string requestRef)
        {
            // TODO: implement FPT polling
            await Task.CompletedTask;
            return null;
        }

        public async Task<VerificationResult> VerifySignedDocumentAsync(byte[] signedPdfBytes)
        {
            await Task.CompletedTask;
            return new VerificationResult { IsValid = false, Status = 0, Details = "FPT verification not implemented." };
        }
    }
}
