using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TPApp.Data;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class SystemParameterService : ISystemParameterService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SystemParameterService> _logger;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

        // Keys containing these substrings are auto-decrypted
        private static readonly string[] SensitiveKeywords = ["PASSWORD", "TOKEN", "ACCOUNT_ID"];

        public SystemParameterService(
            AppDbContext context,
            IConfiguration config,
            IMemoryCache cache,
            ILogger<SystemParameterService> logger)
        {
            _context = context;
            _config  = config;
            _cache   = cache;
            _logger  = logger;
        }

        public async Task<string?> GetAsync(string key)
        {
            var cacheKey = $"sysparam:{key}";

            if (_cache.TryGetValue(cacheKey, out string? cached))
                return cached;

            var param = await _context.SystemParameters
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == key && p.Activated);

            if (param == null)
            {
                // Cache null result briefly to avoid hammering DB for missing keys
                _cache.Set(cacheKey, (string?)null, TimeSpan.FromSeconds(10));
                return null;
            }

            var value = param.Val;

            if (!string.IsNullOrEmpty(value) && IsSensitiveKey(key))
            {
                value = TryDecrypt(value);
                // Do NOT log sensitive values
            }
            else
            {
                _logger.LogDebug("SYS_PARAMETERS[{Key}] = {Value}", key, value);
            }

            _cache.Set(cacheKey, value, CacheDuration);
            return value;
        }

        public async Task<int> GetIntAsync(string key, int defaultValue = 0)
        {
            var raw = await GetAsync(key);
            return int.TryParse(raw, out var result) ? result : defaultValue;
        }

        public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
        {
            var raw = await GetAsync(key);
            if (string.IsNullOrEmpty(raw)) return defaultValue;
            // Accept: "true"/"false", "1"/"0", "yes"/"no"
            return raw.Trim().ToLowerInvariant() is "true" or "1" or "yes";
        }

        public async Task<string> GetRequiredAsync(string key)
        {
            var value = await GetAsync(key);
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    $"Required SYS_PARAMETERS key '{key}' is missing or empty. Please configure it in the database.");
            return value;
        }

        private static bool IsSensitiveKey(string key)
        {
            var upper = key.ToUpperInvariant();
            return SensitiveKeywords.Any(k => upper.Contains(k));
        }

        private string TryDecrypt(string value)
        {
            try
            {
                var encryptKeyword = _config["AppSettings:EncryptKeyword"];
                if (string.IsNullOrEmpty(encryptKeyword)) return value;

                var bytes    = Convert.FromBase64String(value);
                var keyBytes = System.Text.Encoding.UTF8.GetBytes(encryptKeyword);
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] ^= keyBytes[i % keyBytes.Length];

                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // Decryption failed – value may be plain text (not yet encrypted)
                return value;
            }
        }
    }
}
