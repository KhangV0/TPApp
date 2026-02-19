using TPApp.Interfaces;

namespace TPApp.Services.Signing
{
    /// <summary>Skeleton adapter for VNPT CA remote signing.</summary>
    public class VnptSigningProvider : ISigningProvider
    {
        private readonly ISystemParameterService _sysParams;
        private readonly ILogger<VnptSigningProvider> _logger;

        public string ProviderName => "VNPT";

        public VnptSigningProvider(ISystemParameterService sysParams, ILogger<VnptSigningProvider> logger)
        {
            _sysParams = sysParams;
            _logger    = logger;
        }

        public async Task<string> CreateSigningRequestAsync(byte[] pdfBytes, SignerInfo signer, string callbackUrl)
        {
            var apiBase = await _sysParams.GetAsync("SIGNING_VNPT_API_BASE");
            var apiKey  = await _sysParams.GetAsync("SIGNING_VNPT_API_KEY");

            if (string.IsNullOrEmpty(apiBase) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("VNPT signing not configured. Returning stub RequestRef.");
                return $"VNPT-STUB-{Guid.NewGuid():N}";
            }

            // TODO: Implement real VNPT eSign API call using HttpClient
            // POST {apiBase}/api/sign/create  with pdfBytes + signer info + callback
            _logger.LogInformation("VNPT CreateSigningRequest → stub mode.");
            return $"VNPT-{Guid.NewGuid():N}";
        }

        public async Task<SignedResult?> GetSignedDocumentAsync(string requestRef)
        {
            // TODO: GET {apiBase}/api/sign/{requestRef}/result
            _logger.LogInformation("VNPT GetSignedDocument → stub mode (not implemented).");
            await Task.CompletedTask;
            return null;
        }

        public async Task<VerificationResult> VerifySignedDocumentAsync(byte[] signedPdfBytes)
        {
            // TODO: POST {apiBase}/api/verify with PDF bytes
            await Task.CompletedTask;
            return new VerificationResult { IsValid = false, Status = 0, Details = "VNPT verification not implemented." };
        }
    }
}
