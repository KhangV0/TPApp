using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TPApp.Interfaces;

namespace TPApp.Services.Signing
{
    /// <summary>
    /// VNPT SmartCA remote signing adapter.
    /// API: VNPT SmartCA v2 — service/smart-ca/v2/sign
    /// Signing mode controlled by SYS_PARAMETERS.SIGNING_MODE
    /// 
    /// Required SYS_PARAMETERS for PRODUCTION:
    ///   SIGNING_VNPT_API_BASE    = https://smartca.vnpt.vn  (or sandbox URL)
    ///   SIGNING_VNPT_API_KEY     = API key from VNPT SmartCA portal
    ///   SIGNING_VNPT_CREDENTIAL  = Credential ID (cert serial) assigned by VNPT
    /// </summary>
    public class VnptSigningProvider : ISigningProvider
    {
        private readonly ISystemParameterService _sysParams;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<VnptSigningProvider> _logger;

        public string ProviderName => "VNPT";

        public VnptSigningProvider(
            ISystemParameterService sysParams,
            IHttpClientFactory httpFactory,
            ILogger<VnptSigningProvider> logger)
        {
            _sysParams   = sysParams;
            _httpFactory  = httpFactory;
            _logger       = logger;
        }

        public async Task<string> CreateSigningRequestAsync(
            byte[] pdfBytes, SignerInfo signer, string callbackUrl)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";
            _logger.LogInformation("VNPT Signing Mode = {Mode}", mode);

            // ══════════════════════════════════════════════════════════════
            //  PRODUCTION — VNPT SmartCA v2 API
            // ══════════════════════════════════════════════════════════════
            if (mode == "PRODUCTION")
            {
                _logger.LogInformation("VNPT signing running in PRODUCTION mode.");

                var apiBase    = await _sysParams.GetAsync("SIGNING_VNPT_API_BASE");
                var apiKey     = await _sysParams.GetAsync("SIGNING_VNPT_API_KEY");
                var credential = await _sysParams.GetAsync("SIGNING_VNPT_CREDENTIAL");

                if (string.IsNullOrWhiteSpace(apiBase) || string.IsNullOrWhiteSpace(apiKey))
                    throw new InvalidOperationException(
                        "VNPT signing is in PRODUCTION mode but API config missing. " +
                        "Set SIGNING_VNPT_API_BASE and SIGNING_VNPT_API_KEY in SYS_PARAMETERS.");

                var client = _httpFactory.CreateClient();
                client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // VNPT SmartCA v2 signing endpoint
                var payload = new
                {
                    credentialId = credential ?? "",
                    document = new
                    {
                        content      = Convert.ToBase64String(pdfBytes),
                        contentType  = "application/pdf",
                        fileName     = "contract.pdf"
                    },
                    signer = new
                    {
                        fullName     = signer.FullName,
                        email        = signer.Email,
                        phone        = signer.Phone,
                        title        = signer.Title
                    },
                    callbackUrl,
                    signaturePosition = new
                    {
                        page        = -1,   // last page
                        x           = 50,
                        y           = 50,
                        width       = 200,
                        height      = 80
                    }
                };

                var json     = JsonSerializer.Serialize(payload);
                var content  = new StringContent(json, Encoding.UTF8, "application/json");
                var endpoint = $"{apiBase.TrimEnd('/')}/service/smart-ca/v2/sign";

                _logger.LogInformation("VNPT API POST {Endpoint}", endpoint);
                var response = await client.PostAsync(endpoint, content);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("VNPT API error {Status}: {Body}", response.StatusCode, body);
                    throw new InvalidOperationException(
                        $"VNPT SmartCA API returned {(int)response.StatusCode}: {body}");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // VNPT returns: { "requestRef": "...", "status": 0, "message": "..." }
                var requestRef = root.TryGetProperty("requestRef", out var refEl)
                    ? refEl.GetString()
                    : root.TryGetProperty("transactionId", out var txEl)
                        ? txEl.GetString()
                        : null;

                if (string.IsNullOrEmpty(requestRef))
                {
                    _logger.LogError("VNPT API did not return requestRef. Body: {Body}", body);
                    throw new InvalidOperationException("VNPT SmartCA returned no requestRef.");
                }

                _logger.LogInformation("VNPT API returned RequestRef={Ref}", requestRef);
                return requestRef;
            }

            // ══════════════════════════════════════════════════════════════
            //  TESTING — Stub mode with auto mock callback
            // ══════════════════════════════════════════════════════════════
            _logger.LogWarning("VNPT signing running in TESTING mode → stub signing.");

            var stubRef = $"VNPT-STUB-{Guid.NewGuid():N}";

            var baseUrl = callbackUrl.Contains("/Signing/Callback")
                ? callbackUrl[..callbackUrl.IndexOf("/Signing/Callback")]
                : callbackUrl;
            var mockUrl = $"{baseUrl}/Signing/MockCallback";

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3000);
                    var client  = _httpFactory.CreateClient();
                    var mockPayload = new
                    {
                        requestRef     = stubRef,
                        callbackSecret = "",
                        certSerial     = "VNPT-STUB-CERT-001",
                        certSubject    = $"CN={signer.FullName}, O=Stub Corp, L=Hanoi",
                        certIssuer     = "CN=VNPT-CA SHA-256, O=VNPT Group"
                    };
                    var json    = JsonSerializer.Serialize(mockPayload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp    = await client.PostAsync(mockUrl, content);
                    _logger.LogInformation("VNPT stub auto-callback → {Status}", resp.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "VNPT stub auto-callback failed.");
                }
            });

            return stubRef;
        }

        public async Task<SignedResult?> GetSignedDocumentAsync(string requestRef)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";

            if (mode == "PRODUCTION")
            {
                var apiBase = await _sysParams.GetAsync("SIGNING_VNPT_API_BASE");
                var apiKey  = await _sysParams.GetAsync("SIGNING_VNPT_API_KEY");

                if (!string.IsNullOrWhiteSpace(apiBase) && !string.IsNullOrWhiteSpace(apiKey))
                {
                    var client = _httpFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    // VNPT SmartCA get signed document
                    var endpoint = $"{apiBase.TrimEnd('/')}/service/smart-ca/v2/sign/{requestRef}/result";
                    var response = await client.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;

                        return new SignedResult
                        {
                            SignedPdfBytes     = root.TryGetProperty("signedDocument", out var f)
                                ? Convert.FromBase64String(f.GetString() ?? "")
                                : root.TryGetProperty("signedFile", out var f2)
                                    ? Convert.FromBase64String(f2.GetString() ?? "")
                                    : Array.Empty<byte>(),
                            CertificateSerial  = root.TryGetProperty("certSerial", out var cs) ? cs.GetString() : null,
                            CertificateSubject = root.TryGetProperty("certSubject", out var csub) ? csub.GetString() : null,
                            CertificateIssuer  = root.TryGetProperty("certIssuer", out var ci) ? ci.GetString() : null,
                            SignedHash         = root.TryGetProperty("signedHash", out var sh) ? sh.GetString() : null,
                            RawPayload         = body
                        };
                    }
                    _logger.LogWarning("VNPT GetSignedDocument failed: {Status}", response.StatusCode);
                }
            }

            _logger.LogInformation("VNPT GetSignedDocument → not available in TESTING mode.");
            return null;
        }

        public async Task<VerificationResult> VerifySignedDocumentAsync(byte[] signedPdfBytes)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";

            if (mode == "PRODUCTION")
            {
                var apiBase = await _sysParams.GetAsync("SIGNING_VNPT_API_BASE");
                var apiKey  = await _sysParams.GetAsync("SIGNING_VNPT_API_KEY");

                if (!string.IsNullOrWhiteSpace(apiBase) && !string.IsNullOrWhiteSpace(apiKey))
                {
                    var client = _httpFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    var payload = new { document = Convert.ToBase64String(signedPdfBytes) };
                    var json    = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var endpoint = $"{apiBase.TrimEnd('/')}/service/smart-ca/v2/verify";
                    var response = await client.PostAsync(endpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        return new VerificationResult
                        {
                            IsValid = root.TryGetProperty("isValid", out var iv) && iv.GetBoolean(),
                            Status  = root.TryGetProperty("status", out var st) ? st.GetInt32() : 0,
                            Details = body
                        };
                    }
                    _logger.LogWarning("VNPT VerifySignedDocument failed: {Status}", response.StatusCode);
                }
            }

            return new VerificationResult
            {
                IsValid = false, Status = 0,
                Details = mode == "PRODUCTION"
                    ? "VNPT SmartCA verification API call failed."
                    : "Verification not available in TESTING mode."
            };
        }
    }
}
