using System.Security.Cryptography;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class HashService : IHashService
    {
        public string ComputeSha256(Stream stream)
        {
            stream.Position = 0;
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public string ComputeSha256(byte[] bytes)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
        }
    }
}
