using TechMart.LocalSigner.Services;
using Net.Pkcs11Interop.Common;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ───
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHostedService<TokenMonitorService>();

// ─── CORS — restrict to allowed origins ───
var allowedOrigins = builder.Configuration.GetSection("Server:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ─── Configure port ───
var port = builder.Configuration.GetValue("Server:Port", 15800);
builder.WebHost.UseUrls($"http://localhost:{port}");

var app = builder.Build();
app.UseCors();

// ════════════════════════════════════════════════════════════
//  API Endpoints
// ════════════════════════════════════════════════════════════

// GET /health — check agent + token status
app.MapGet("/health", (TokenService svc) =>
{
    return Results.Ok(new
    {
        status = "ok",
        agent = "TechMart.LocalSigner",
        version = "1.0.0",
        tokenDetected = svc.TokenDetected,
        driverLoaded = svc.DriverPath != null,
        driverPath = svc.DriverPath
    });
});

// GET /certificates — list certs on USB Token
app.MapGet("/certificates", (TokenService svc) =>
{
    if (!svc.TokenDetected)
        return Results.Json(new { error = "No USB Token detected" }, statusCode: 404);

    var certs = svc.ListCertificates();
    return Results.Ok(certs.Select(c => new
    {
        c.Subject,
        c.Issuer,
        c.SerialNumber,
        c.NotBefore,
        c.NotAfter,
        c.Label,
        c.Thumbprint,
        isValid = c.NotAfter > DateTime.Now
    }));
});

// POST /sign — sign hash with private key on token
app.MapPost("/sign", (SignRequest req, TokenService svc, ILogger<Program> log) =>
{
    if (string.IsNullOrEmpty(req.Base64Hash))
        return Results.BadRequest(new { error = "base64Hash is required" });
    if (string.IsNullOrEmpty(req.CertificateSerial))
        return Results.BadRequest(new { error = "certificateSerial is required" });
    if (string.IsNullOrEmpty(req.Pin))
        return Results.BadRequest(new { error = "pin is required" });

    if (!svc.TokenDetected)
        return Results.Json(new { error = "No USB Token detected" }, statusCode: 404);

    try
    {
        var hash = Convert.FromBase64String(req.Base64Hash);
        log.LogInformation("🔐 Signing hash ({Len} bytes) with cert {Serial}",
            hash.Length, req.CertificateSerial);

        var signature = svc.SignHash(hash, req.CertificateSerial, req.Pin);
        var certBytes = svc.GetCertificateBytes(req.CertificateSerial);

        log.LogInformation("✅ Signature created: {Len} bytes", signature.Length);

        return Results.Ok(new
        {
            signature = Convert.ToBase64String(signature),
            certificate = certBytes != null ? Convert.ToBase64String(certBytes) : null
        });
    }
    catch (Net.Pkcs11Interop.Common.Pkcs11Exception pkcsEx) when (pkcsEx.RV == CKR.CKR_PIN_INCORRECT)
    {
        log.LogWarning("❌ Incorrect PIN");
        return Results.Json(new { error = "Mã PIN không đúng" }, statusCode: 401);
    }
    catch (Net.Pkcs11Interop.Common.Pkcs11Exception pkcsEx) when (pkcsEx.RV == CKR.CKR_PIN_LOCKED)
    {
        log.LogError("🔒 Token PIN is locked!");
        return Results.Json(new { error = "Token đã bị khóa do nhập sai PIN quá nhiều lần" }, statusCode: 403);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Signing failed");
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

// ─── Start ───
app.Logger.LogInformation("🚀 TechMart LocalSigner starting on http://localhost:{Port}", port);
app.Run();

// ─── Request DTOs ───
record SignRequest(string? Base64Hash, string? CertificateSerial, string? Pin);
