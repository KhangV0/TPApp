using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TPApp.Interfaces;

namespace TPApp.Services.Signing
{
    /// <summary>
    /// Viettel MySign (Cloud-CA) remote signing adapter.
    /// API: Viettel MySign — /mysign/api/v1/signature/signHash
    /// Signing mode controlled by SYS_PARAMETERS.SIGNING_MODE
    /// 
    /// Required SYS_PARAMETERS for PRODUCTION:
    ///   SIGNING_VIETTEL_API_BASE   = https://mysign.viettel-ca.vn  (or sandbox URL)
    ///   SIGNING_VIETTEL_CLIENT_ID  = Client ID from Viettel-CA
    ///   SIGNING_VIETTEL_SECRET     = Client Secret from Viettel-CA
    ///   SIGNING_VIETTEL_USER_ID    = Service account user_id (optional, for server-to-server)
    /// </summary>
    public class ViettelSigningProvider : ISigningProvider
    {
        private readonly ISystemParameterService _sysParams;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<ViettelSigningProvider> _logger;

        public string ProviderName => "Viettel";

        public ViettelSigningProvider(
            ISystemParameterService sysParams,
            IHttpClientFactory httpFactory,
            ILogger<ViettelSigningProvider> logger)
        {
            _sysParams   = sysParams;
            _httpFactory  = httpFactory;
            _logger       = logger;
        }

        /// <summary>Obtain access token from Viettel MySign OAuth2.</summary>
        private async Task<string?> GetAccessTokenAsync(HttpClient client, string apiBase)
        {
            var clientId = await _sysParams.GetAsync("SIGNING_VIETTEL_CLIENT_ID");
            var secret   = await _sysParams.GetAsync("SIGNING_VIETTEL_SECRET");
            var userId   = await _sysParams.GetAsync("SIGNING_VIETTEL_USER_ID");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(secret))
                return null;

            var loginPayload = new
            {
                client_id     = clientId,
                client_secret = secret,
                user_id       = userId ?? "",
                grant_type    = "client_credentials"
            };

            var json    = JsonSerializer.Serialize(loginPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var loginResp = await client.PostAsync($"{apiBase.TrimEnd('/')}/mysign/api/v1/login", content);
            if (!loginResp.IsSuccessStatusCode)
            {
                _logger.LogError("Viettel MySign login failed: {Status}", loginResp.StatusCode);
                return null;
            }

            var loginBody = await loginResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(loginBody);
            return doc.RootElement.TryGetProperty("access_token", out var at)
                ? at.GetString()
                : doc.RootElement.TryGetProperty("token", out var tk)
                    ? tk.GetString()
                    : null;
        }

        public async Task<string> CreateSigningRequestAsync(
            byte[] pdfBytes, SignerInfo signer, string callbackUrl)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";
            _logger.LogInformation("Viettel Signing Mode = {Mode}", mode);

            // ══════════════════════════════════════════════════════════════
            //  PRODUCTION — Viettel MySign API
            // ══════════════════════════════════════════════════════════════
            if (mode == "PRODUCTION")
            {
                _logger.LogInformation("Viettel signing running in PRODUCTION mode.");

                var apiBase  = await _sysParams.GetAsync("SIGNING_VIETTEL_API_BASE");
                var clientId = await _sysParams.GetAsync("SIGNING_VIETTEL_CLIENT_ID");
                var secret   = await _sysParams.GetAsync("SIGNING_VIETTEL_SECRET");

                if (string.IsNullOrWhiteSpace(apiBase) ||
                    string.IsNullOrWhiteSpace(clientId) ||
                    string.IsNullOrWhiteSpace(secret))
                    throw new InvalidOperationException(
                        "Viettel signing is in PRODUCTION mode but API config missing. " +
                        "Set SIGNING_VIETTEL_API_BASE, SIGNING_VIETTEL_CLIENT_ID, and SIGNING_VIETTEL_SECRET in SYS_PARAMETERS.");

                var client = _httpFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Step 1: Login to get access token
                var accessToken = await GetAccessTokenAsync(client, apiBase);
                if (string.IsNullOrEmpty(accessToken))
                    throw new InvalidOperationException("Viettel MySign authentication failed.");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Step 2: List credentials to get credentialId
                var credEndpoint = $"{apiBase.TrimEnd('/')}/mysign/api/v1/credentials/list";
                var credResp = await client.GetAsync(credEndpoint);
                string? credentialId = null;

                if (credResp.IsSuccessStatusCode)
                {
                    var credBody = await credResp.Content.ReadAsStringAsync();
                    using var credDoc = JsonDocument.Parse(credBody);
                    if (credDoc.RootElement.TryGetProperty("credentialIds", out var creds) &&
                        creds.GetArrayLength() > 0)
                    {
                        credentialId = creds[0].GetString();
                    }
                }
                _logger.LogInformation("Viettel credentialId = {Id}", credentialId ?? "(none)");

                // Step 3: Request OTP authorization for signing
                if (!string.IsNullOrEmpty(credentialId))
                {
                    var authPayload = new { credentialId };
                    var authJson    = JsonSerializer.Serialize(authPayload);
                    var authContent = new StringContent(authJson, Encoding.UTF8, "application/json");
                    var authEndpoint = $"{apiBase.TrimEnd('/')}/mysign/api/v1/credentials/sendOTP";
                    await client.PostAsync(authEndpoint, authContent);
                    _logger.LogInformation("Viettel OTP authorization requested for credential {Id}", credentialId);
                }

                // Step 4: Create signing request (signHash)
                var payload = new
                {
                    credentialId = credentialId ?? "",
                    document = new
                    {
                        content     = Convert.ToBase64String(pdfBytes),
                        contentType = "application/pdf",
                        fileName    = "contract.pdf"
                    },
                    signer = new
                    {
                        name  = signer.FullName,
                        email = signer.Email,
                        phone = signer.Phone
                    },
                    callbackUrl,
                    signaturePosition = new
                    {
                        page   = -1,
                        x      = 50,
                        y      = 50,
                        width  = 200,
                        height = 80
                    }
                };

                var json     = JsonSerializer.Serialize(payload);
                var content  = new StringContent(json, Encoding.UTF8, "application/json");
                var endpoint = $"{apiBase.TrimEnd('/')}/mysign/api/v1/signature/signHash";

                _logger.LogInformation("Viettel API POST {Endpoint}", endpoint);
                var response = await client.PostAsync(endpoint, content);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Viettel API error {Status}: {Body}", response.StatusCode, body);
                    throw new InvalidOperationException(
                        $"Viettel MySign API returned {(int)response.StatusCode}: {body}");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var requestRef = root.TryGetProperty("transactionId", out var txEl)
                    ? txEl.GetString()
                    : root.TryGetProperty("requestRef", out var refEl)
                        ? refEl.GetString()
                        : root.TryGetProperty("requestId", out var ridEl)
                            ? ridEl.GetString()
                            : null;

                if (string.IsNullOrEmpty(requestRef))
                {
                    _logger.LogError("Viettel API did not return transactionId. Body: {Body}", body);
                    throw new InvalidOperationException("Viettel MySign returned no transactionId.");
                }

                _logger.LogInformation("Viettel API returned RequestRef={Ref}", requestRef);
                return requestRef;
            }

            // ══════════════════════════════════════════════════════════════
            //  TESTING — Stub mode with auto mock callback
            // ══════════════════════════════════════════════════════════════
            _logger.LogWarning("Viettel signing running in TESTING mode → stub signing.");

            var stubRef = $"VIETTEL-STUB-{Guid.NewGuid():N}";

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
                        certSerial     = "VIETTEL-STUB-CERT-001",
                        certSubject    = $"CN={signer.FullName}, O=Stub Corp, L=Hanoi",
                        certIssuer     = "CN=Viettel-CA SHA-256, O=Viettel Group"
                    };
                    var json    = JsonSerializer.Serialize(mockPayload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp    = await client.PostAsync(mockUrl, content);
                    _logger.LogInformation("Viettel stub auto-callback → {Status}", resp.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Viettel stub auto-callback failed.");
                }
            });

            return stubRef;
        }

        public async Task<SignedResult?> GetSignedDocumentAsync(string requestRef)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";

            if (mode == "PRODUCTION")
            {
                var apiBase = await _sysParams.GetAsync("SIGNING_VIETTEL_API_BASE");

                if (!string.IsNullOrWhiteSpace(apiBase))
                {
                    var client = _httpFactory.CreateClient();
                    var accessToken = await GetAccessTokenAsync(client, apiBase);
                    if (!string.IsNullOrEmpty(accessToken))
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);

                    var endpoint = $"{apiBase.TrimEnd('/')}/mysign/api/v1/signature/{requestRef}/document";
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
                    _logger.LogWarning("Viettel GetSignedDocument failed: {Status}", response.StatusCode);
                }
            }

            _logger.LogInformation("Viettel GetSignedDocument → not available in TESTING mode.");
            return null;
        }

        public async Task<VerificationResult> VerifySignedDocumentAsync(byte[] signedPdfBytes)
        {
            var mode = (await _sysParams.GetAsync("SIGNING_MODE"))?.Trim().ToUpperInvariant() ?? "TESTING";

            if (mode == "PRODUCTION")
            {
                var apiBase = await _sysParams.GetAsync("SIGNING_VIETTEL_API_BASE");

                if (!string.IsNullOrWhiteSpace(apiBase))
                {
                    var client = _httpFactory.CreateClient();
                    var accessToken = await GetAccessTokenAsync(client, apiBase);
                    if (!string.IsNullOrEmpty(accessToken))
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);

                    var payload = new { document = Convert.ToBase64String(signedPdfBytes) };
                    var json    = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var endpoint = $"{apiBase.TrimEnd('/')}/mysign/api/v1/signature/verify";
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
                    _logger.LogWarning("Viettel VerifySignedDocument failed: {Status}", response.StatusCode);
                }
            }

            return new VerificationResult
            {
                IsValid = false, Status = 0,
                Details = mode == "PRODUCTION"
                    ? "Viettel MySign verification API call failed."
                    : "Verification not available in TESTING mode."
            };
        }
    }
}
