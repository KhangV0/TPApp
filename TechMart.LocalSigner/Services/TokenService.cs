using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Security.Cryptography.X509Certificates;

namespace TechMart.LocalSigner.Services;

/// <summary>
/// PKCS#11 Token Service — communicates with VNPT USB Token
/// </summary>
public class TokenService : IDisposable
{
    private readonly ILogger<TokenService> _log;
    private readonly string[] _driverPaths;
    private IPkcs11Library? _pkcs11;
    private string? _loadedDriver;

    public bool TokenDetected { get; private set; }
    public string? DriverPath => _loadedDriver;

    public TokenService(IConfiguration config, ILogger<TokenService> log)
    {
        _log = log;
        _driverPaths = config.GetSection("Pkcs11:DriverPaths").Get<string[]>() ?? [];
    }

    // ────────────────────────────────────────────────────────
    //  Initialize — find and load the first available driver
    // ────────────────────────────────────────────────────────
    public bool Initialize()
    {
        foreach (var path in _driverPaths)
        {
            if (!File.Exists(path)) continue;
            try
            {
                _log.LogInformation("Loading PKCS#11 driver: {Path}", path);
                var factories = new Pkcs11InteropFactories();
                _pkcs11 = factories.Pkcs11LibraryFactory.LoadPkcs11Library(
                    factories, path, AppType.MultiThreaded);
                _loadedDriver = path;
                _log.LogInformation("✅ PKCS#11 driver loaded: {Path}", path);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogWarning("⚠️ Cannot load {Path}: {Msg}", path, ex.Message);
            }
        }
        _log.LogError("❌ No PKCS#11 driver found");
        return false;
    }

    // ────────────────────────────────────────────────────────
    //  Scan — check if token is present
    // ────────────────────────────────────────────────────────
    public bool ScanToken()
    {
        if (_pkcs11 == null) return false;
        try
        {
            var slots = _pkcs11.GetSlotList(SlotsType.WithTokenPresent);
            TokenDetected = slots.Count > 0;
            return TokenDetected;
        }
        catch (Exception ex)
        {
            _log.LogWarning("Token scan error: {Msg}", ex.Message);
            TokenDetected = false;
            return false;
        }
    }

    // ────────────────────────────────────────────────────────
    //  List certificates on the token
    // ────────────────────────────────────────────────────────
    public List<CertificateInfo> ListCertificates()
    {
        var result = new List<CertificateInfo>();
        if (_pkcs11 == null) return result;

        var slots = _pkcs11.GetSlotList(SlotsType.WithTokenPresent);
        foreach (var slot in slots)
        {
            using var session = slot.OpenSession(SessionType.ReadOnly);
            // Find certificate objects
            var searchAttrs = new List<IObjectAttribute>
            {
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
            };
            var objects = session.FindAllObjects(searchAttrs);

            foreach (var obj in objects)
            {
                try
                {
                    var attrs = session.GetAttributeValue(obj, new List<CKA>
                    {
                        CKA.CKA_VALUE,
                        CKA.CKA_LABEL,
                        CKA.CKA_ID
                    });

                    var certBytes = attrs[0].GetValueAsByteArray();
                    var label = attrs[1].GetValueAsString();
                    var id = attrs[2].GetValueAsByteArray();

                    if (certBytes == null || certBytes.Length == 0) continue;

                    var x509 = X509CertificateLoader.LoadCertificate(certBytes);
                    result.Add(new CertificateInfo
                    {
                        Subject = x509.Subject,
                        Issuer = x509.Issuer,
                        SerialNumber = x509.SerialNumber,
                        NotBefore = x509.NotBefore,
                        NotAfter = x509.NotAfter,
                        Label = label ?? "",
                        CkaId = Convert.ToBase64String(id ?? []),
                        Thumbprint = x509.Thumbprint
                    });
                }
                catch (Exception ex)
                {
                    _log.LogWarning("Error reading cert object: {Msg}", ex.Message);
                }
            }
        }
        _log.LogInformation("Found {Count} certificate(s) on token", result.Count);
        return result;
    }

    // ────────────────────────────────────────────────────────
    //  Sign hash using private key on token
    // ────────────────────────────────────────────────────────
    public byte[] SignHash(byte[] hash, string certificateSerial, string pin)
    {
        if (_pkcs11 == null)
            throw new InvalidOperationException("PKCS#11 not initialized");

        var slots = _pkcs11.GetSlotList(SlotsType.WithTokenPresent);
        if (slots.Count == 0)
            throw new InvalidOperationException("No USB Token detected");

        var slot = slots[0];
        using var session = slot.OpenSession(SessionType.ReadWrite);

        // Login with PIN
        _log.LogInformation("Logging in to token with PIN...");
        session.Login(CKU.CKU_USER, pin);

        try
        {
            // Find private key matching certificate serial
            var keyHandle = FindPrivateKey(session, certificateSerial);

            // Sign using CKM_RSA_PKCS (raw RSA, hash already computed)
            // We need to wrap hash in DigestInfo for SHA-256
            var digestInfo = WrapInDigestInfo(hash);

            var mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
            var signature = session.Sign(mechanism, keyHandle, digestInfo);

            _log.LogInformation("✅ Signing completed, signature length: {Len} bytes", signature.Length);
            return signature;
        }
        finally
        {
            try { session.Logout(); } catch { }
        }
    }

    // ────────────────────────────────────────────────────────
    //  Get certificate bytes by serial (for server embedding)
    // ────────────────────────────────────────────────────────
    public byte[]? GetCertificateBytes(string serialNumber)
    {
        if (_pkcs11 == null) return null;

        var slots = _pkcs11.GetSlotList(SlotsType.WithTokenPresent);
        foreach (var slot in slots)
        {
            using var session = slot.OpenSession(SessionType.ReadOnly);
            var searchAttrs = new List<IObjectAttribute>
            {
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
            };
            var objects = session.FindAllObjects(searchAttrs);

            foreach (var obj in objects)
            {
                var attrs = session.GetAttributeValue(obj, new List<CKA> { CKA.CKA_VALUE });
                var certBytes = attrs[0].GetValueAsByteArray();
                if (certBytes == null) continue;

                var x509 = X509CertificateLoader.LoadCertificate(certBytes);
                if (x509.SerialNumber.Equals(serialNumber, StringComparison.OrdinalIgnoreCase))
                    return certBytes;
            }
        }
        return null;
    }

    // ─── Find private key matching a certificate serial ───
    private IObjectHandle FindPrivateKey(Net.Pkcs11Interop.HighLevelAPI.ISession session, string certificateSerial)
    {
        // First find the cert to get its CKA_ID
        var certSearchAttrs = new List<IObjectAttribute>
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
        };
        var certs = session.FindAllObjects(certSearchAttrs);
        byte[]? matchingId = null;

        foreach (var cert in certs)
        {
            var attrs = session.GetAttributeValue(cert, new List<CKA> { CKA.CKA_VALUE, CKA.CKA_ID });
            var certBytes = attrs[0].GetValueAsByteArray();
            var id = attrs[1].GetValueAsByteArray();

            if (certBytes == null) continue;
            var x509 = X509CertificateLoader.LoadCertificate(certBytes);
            if (x509.SerialNumber.Equals(certificateSerial, StringComparison.OrdinalIgnoreCase))
            {
                matchingId = id;
                break;
            }
        }

        if (matchingId == null)
            throw new InvalidOperationException($"Certificate {certificateSerial} not found on token");

        // Find private key with same CKA_ID
        var keySearchAttrs = new List<IObjectAttribute>
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, matchingId)
        };
        var keys = session.FindAllObjects(keySearchAttrs);

        if (keys.Count == 0)
            throw new InvalidOperationException("Private key not found for certificate");

        _log.LogInformation("Private key found for serial: {Serial}", certificateSerial);
        return keys[0];
    }

    // ─── Wrap SHA-256 hash in ASN.1 DigestInfo ───
    private static byte[] WrapInDigestInfo(byte[] hash)
    {
        // SHA-256 DigestInfo prefix (DER encoded)
        byte[] prefix = [
            0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86,
            0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05,
            0x00, 0x04, 0x20
        ];
        var digestInfo = new byte[prefix.Length + hash.Length];
        Buffer.BlockCopy(prefix, 0, digestInfo, 0, prefix.Length);
        Buffer.BlockCopy(hash, 0, digestInfo, prefix.Length, hash.Length);
        return digestInfo;
    }

    public void Dispose()
    {
        _pkcs11?.Dispose();
    }
}

// ─── DTOs ───
public class CertificateInfo
{
    public string Subject { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
    public string Label { get; set; } = "";
    public string CkaId { get; set; } = "";
    public string Thumbprint { get; set; } = "";
}
