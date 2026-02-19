using TPApp.Interfaces;

namespace TPApp.Services.Signing
{
    /// <summary>Skeleton adapter for Viettel CA remote signing.</summary>
    public class ViettelSigningProvider : ISigningProvider
    {
        private readonly ISystemParameterService _sysParams;
        private readonly ILogger<ViettelSigningProvider> _logger;

        public string ProviderName => "Viettel";

        public ViettelSigningProvider(ISystemParameterService sysParams, ILogger<ViettelSigningProvider> logger)
        {
            _sysParams = sysParams;
            _logger    = logger;
        }

        public async Task<string> CreateSigningRequestAsync(byte[] pdfBytes, SignerInfo signer, string callbackUrl)
        {
            var apiBase = await _sysParams.GetAsync("SIGNING_VIETTEL_API_BASE");
            var apiKey  = await _sysParams.GetAsync("SIGNING_VIETTEL_API_KEY");

            if (string.IsNullOrEmpty(apiBase) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Viettel signing not configured. Returning stub RequestRef.");
                return $"VIETTEL-STUB-{Guid.NewGuid():N}";
            }

            // TODO: Implement real Viettel CA signing API
            _logger.LogInformation("Viettel CreateSigningRequest → stub mode.");
            return $"VIETTEL-{Guid.NewGuid():N}";
        }

        public async Task<SignedResult?> GetSignedDocumentAsync(string requestRef)
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task<VerificationResult> VerifySignedDocumentAsync(byte[] signedPdfBytes)
        {
            await Task.CompletedTask;
            return new VerificationResult { IsValid = false, Status = 0, Details = "Viettel verification not implemented." };
        }
    }
}
