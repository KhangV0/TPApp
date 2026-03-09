using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TPApp.Interfaces;

namespace TPApp.Services.Signing
{
    /// <summary>
    /// FPT.eSign / FPT Signing Hub remote signing adapter.
    /// API: FPT Signing Hub — /api/v1/signing/request
    /// Signing mode controlled by SYS_PARAMETERS.SIGNING_MODE
    /// 
    /// Required SYS_PARAMETERS for PRODUCTION:
    ///   SIGNING_FPT_API_BASE    = https://signinghub.fpt.com.vn  (or sandbox URL)
    ///   SIGNING_FPT_API_KEY     = API key from FPT Signing Hub
    ///   SIGNING_FPT_CLIENT_ID   = OAuth2 Client ID (if required)
    ///   SIGNING_FPT_SECRET      = OAuth2 Client Secret (if required)
    /// </summary>
    public class FptSigningProvider : ISigningProvider
    {
        private readonly ISystemParameterService _sysParams;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<FptSigningProvider> _logger;

        public string ProviderName => "FPT";

        public FptSigningProvider(
            ISystemParameterService sysParams,
            IHttpClientFactory httpFactory,
            ILogger<FptSigningProvider> logger)
        {
            _sysParams   = sysParams;
            _httpFactory  = httpFactory;
            _logger       = logger;
        }

        /// <summary>Obtain OAuth2 access token from FPT (if clientId/secret configured).</summary>
        private async Task<string?> GetAccessTokenAsync(HttpClient client, string apiBase)
        {
            var clientId = await _sysParams.GetAsync("SIGNING_FPT_CLIENT_ID");
            var secret   = await _sysParams.GetAsync("SIGNING_FPT_SECRET");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(secret))
                return null;

            var tokenPayload = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", secret),
                new KeyValuePair<string, string>("scope", "signing")
            });

            var tokenResp = await client.PostAsync($"{apiBase.TrimEnd('/')}/api/v1/auth/token", tokenPayload);
            if (!tokenResp.IsSuccessStatusCode)
            {
                _logger.LogError("FPT OAuth2 token request failed: {Status}", tokenResp.StatusCode);
                return null;
            }

            var tokenBody = await tokenResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(tokenBody);
            return doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
        }

        public async Task<string> CreateSigningRequestAsync(
            byte[] pdfBytes, SignerInfo signer, string callbackUrl)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";
            _logger.LogInformation("FPT Signing Mode = {Mode}", mode);

            // ══════════════════════════════════════════════════════════════
            //  PRODUCTION — FPT Signing Hub API
            // ══════════════════════════════════════════════════════════════
            if (mode == "PRODUCTION")
            {
                _logger.LogInformation("FPT signing running in PRODUCTION mode.");

                var apiBase = await _sysParams.GetAsync("SIGNING_FPT_API_BASE");
                var apiKey  = await _sysParams.GetAsync("SIGNING_FPT_API_KEY");

                if (string.IsNullOrWhiteSpace(apiBase) || string.IsNullOrWhiteSpace(apiKey))
                    throw new InvalidOperationException(
                        "FPT signing is in PRODUCTION mode but API config missing. " +
                        "Set SIGNING_FPT_API_BASE and SIGNING_FPT_API_KEY in SYS_PARAMETERS.");

                var client = _httpFactory.CreateClient();
                client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Attempt OAuth2 authentication
                var accessToken = await GetAccessTokenAsync(client, apiBase);
                if (!string.IsNullOrEmpty(accessToken))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);

                // FPT Signing Hub create signing request
                var payload = new
                {
                    document = new
                    {
                        content     = Convert.ToBase64String(pdfBytes),
                        contentType = "application/pdf",
                        fileName    = "contract.pdf"
                    },
                    signers = new[]
                    {
                        new
                        {
                            name         = signer.FullName,
                            email        = signer.Email,
                            phone        = signer.Phone,
                            signingOrder = 1,
                            signatureType = "REMOTE"   // remote CA signing
                        }
                    },
                    callbackUrl,
                    signatureAppearance = new
                    {
                        page   = -1,   // last page
                        x      = 50,
                        y      = 50,
                        width  = 200,
                        height = 80,
                        showSignerName = true,
                        showDate       = true
                    }
                };

                var json     = JsonSerializer.Serialize(payload);
                var content  = new StringContent(json, Encoding.UTF8, "application/json");
                var endpoint = $"{apiBase.TrimEnd('/')}/api/v1/signing/request";

                _logger.LogInformation("FPT API POST {Endpoint}", endpoint);
                var response = await client.PostAsync(endpoint, content);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FPT API error {Status}: {Body}", response.StatusCode, body);
                    throw new InvalidOperationException(
                        $"FPT Signing Hub API returned {(int)response.StatusCode}: {body}");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var requestRef = root.TryGetProperty("requestId", out var ridEl)
                    ? ridEl.GetString()
                    : root.TryGetProperty("requestRef", out var refEl)
                        ? refEl.GetString()
                        : root.TryGetProperty("transactionId", out var txEl)
                            ? txEl.GetString()
                            : null;

                if (string.IsNullOrEmpty(requestRef))
                {
                    _logger.LogError("FPT API did not return requestId. Body: {Body}", body);
                    throw new InvalidOperationException("FPT Signing Hub returned no requestId.");
                }

                _logger.LogInformation("FPT API returned RequestRef={Ref}", requestRef);
                return requestRef;
            }

            // ══════════════════════════════════════════════════════════════
            //  TESTING — Stub mode with auto mock callback
            // ══════════════════════════════════════════════════════════════
            _logger.LogWarning("FPT signing running in TESTING mode → stub signing.");

            var stubRef = $"FPT-STUB-{Guid.NewGuid():N}";

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
                        certSerial     = "FPT-STUB-CERT-001",
                        certSubject    = $"CN={signer.FullName}, O=Stub Corp, L=HCMC",
                        certIssuer     = "CN=FPT-CA SHA-256, O=FPT Information System"
                    };
                    var json    = JsonSerializer.Serialize(mockPayload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp    = await client.PostAsync(mockUrl, content);
                    _logger.LogInformation("FPT stub auto-callback → {Status}", resp.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FPT stub auto-callback failed.");
                }
            });

            return stubRef;
        }

        public async Task<SignedResult?> GetSignedDocumentAsync(string requestRef)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";

            if (mode == "PRODUCTION")
            {
                var apiBase = await _sysParams.GetAsync("SIGNING_FPT_API_BASE");
                var apiKey  = await _sysParams.GetAsync("SIGNING_FPT_API_KEY");

                if (!string.IsNullOrWhiteSpace(apiBase) && !string.IsNullOrWhiteSpace(apiKey))
                {
                    var client = _httpFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    var accessToken = await GetAccessTokenAsync(client, apiBase);
                    if (!string.IsNullOrEmpty(accessToken))
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);

                    var endpoint = $"{apiBase.TrimEnd('/')}/api/v1/signing/{requestRef}/document";
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
                                : root.TryGetProperty("content", out var f2)
                                    ? Convert.FromBase64String(f2.GetString() ?? "")
                                    : Array.Empty<byte>(),
                            CertificateSerial  = root.TryGetProperty("certSerial", out var cs) ? cs.GetString() : null,
                            CertificateSubject = root.TryGetProperty("certSubject", out var csub) ? csub.GetString() : null,
                            CertificateIssuer  = root.TryGetProperty("certIssuer", out var ci) ? ci.GetString() : null,
                            SignedHash         = root.TryGetProperty("signedHash", out var sh) ? sh.GetString() : null,
                            RawPayload         = body
                        };
                    }
                    _logger.LogWarning("FPT GetSignedDocument failed: {Status}", response.StatusCode);
                }
            }

            _logger.LogInformation("FPT GetSignedDocument → not available in TESTING mode.");
            return null;
        }

        public async Task<VerificationResult> VerifySignedDocumentAsync(byte[] signedPdfBytes)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";

            if (mode == "PRODUCTION")
            {
                var apiBase = await _sysParams.GetAsync("SIGNING_FPT_API_BASE");
                var apiKey  = await _sysParams.GetAsync("SIGNING_FPT_API_KEY");

                if (!string.IsNullOrWhiteSpace(apiBase) && !string.IsNullOrWhiteSpace(apiKey))
                {
                    var client = _httpFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    var accessToken = await GetAccessTokenAsync(client, apiBase);
                    if (!string.IsNullOrEmpty(accessToken))
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);

                    var payload = new { document = Convert.ToBase64String(signedPdfBytes) };
                    var json    = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var endpoint = $"{apiBase.TrimEnd('/')}/api/v1/signing/verify";
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
                    _logger.LogWarning("FPT VerifySignedDocument failed: {Status}", response.StatusCode);
                }
            }

            return new VerificationResult
            {
                IsValid = false, Status = 0,
                Details = mode == "PRODUCTION"
                    ? "FPT Signing Hub verification API call failed."
                    : "Verification not available in TESTING mode."
            };
        }
    }
}
