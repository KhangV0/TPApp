using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TPApp.Data;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class CmsAccessService : ICmsAccessService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public CmsAccessService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> CanAccessCmsAsync(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return false;

            var cacheKey = $"CmsAccess_{userId}";
            if (_cache.TryGetValue(cacheKey, out bool cached))
                return cached;

            var dbUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            var result = dbUser?.IsAdmin == true;

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }
    }
}
