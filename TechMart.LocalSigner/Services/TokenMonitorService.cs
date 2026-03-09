namespace TechMart.LocalSigner.Services;

/// <summary>
/// Background service that periodically scans for USB Token presence
/// </summary>
public class TokenMonitorService : BackgroundService
{
    private readonly TokenService _tokenService;
    private readonly ILogger<TokenMonitorService> _log;
    private readonly int _intervalMs;

    public TokenMonitorService(TokenService tokenService, IConfiguration config, 
        ILogger<TokenMonitorService> log)
    {
        _tokenService = tokenService;
        _log = log;
        _intervalMs = config.GetValue("Pkcs11:TokenScanIntervalMs", 3000);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("🔌 Token monitor started (interval: {Ms}ms)", _intervalMs);

        // Initialize PKCS#11 driver
        if (!_tokenService.Initialize())
        {
            _log.LogError("❌ PKCS#11 initialization failed. Token monitor will keep retrying...");
        }

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var detected = _tokenService.ScanToken();
                if (detected)
                    _log.LogDebug("Token present ✅");
            }
            catch (Exception ex)
            {
                _log.LogWarning("Token scan error: {Msg}", ex.Message);
            }

            await Task.Delay(_intervalMs, ct);
        }
    }
}
