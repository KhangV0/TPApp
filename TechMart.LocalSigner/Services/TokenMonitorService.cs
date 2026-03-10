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

        while (!ct.IsCancellationRequested)
        {
            // Re-try Initialize every loop until a driver is found
            if (_tokenService.DriverPath == null)
            {
                if (_tokenService.Initialize())
                    _log.LogInformation("✅ PKCS#11 driver loaded successfully.");
                else
                    _log.LogWarning("⚠️ No PKCS#11 driver found yet. " +
                        "Please install the USB Token driver (VNPT-CA / eToken) and replug the device.");
            }
            else
            {
                // Driver loaded — scan for token presence
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
            }

            await Task.Delay(_intervalMs, ct);
        }
    }
}
