using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TPApp.Areas.Cms.Models;
using TPApp.Data;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class CntbMasterService : ICntbMasterService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public CntbMasterService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public Task<List<LookupDto>> GetXuatXuAsync()
            => GetOrCreateAsync("cntb_xuatxu", () =>
                _context.XuatXus.AsNoTracking()
                    .OrderBy(x => x.Title)
                    .Select(x => new LookupDto { Id = x.Id, Title = x.Title ?? "" })
                    .ToListAsync());

        public Task<List<LookupDto>> GetMucDoAsync()
            => GetOrCreateAsync("cntb_mucdo", () =>
                _context.MucDos.AsNoTracking()
                    .OrderBy(x => x.Title)
                    .Select(x => new LookupDto { Id = x.Id, Title = x.Title ?? "" })
                    .ToListAsync());

        public Task<List<LookupDto>> GetLinhVucAsync()
            => GetOrCreateAsync("cntb_linhvuc", () =>
                _context.Categories.AsNoTracking()
                    .Where(c => c.ParentId == 1 && c.StatusId == 1)
                    .OrderBy(c => c.Title)
                    .Select(c => new LookupDto { Id = c.CatId, Title = c.Title ?? "" })
                    .ToListAsync());

        public Task<List<LookupDto>> GetRootSitesAsync()
            => GetOrCreateAsync("cntb_rootsites", () =>
                _context.RootSites.AsNoTracking()
                    .OrderBy(r => r.SiteName)
                    .Select(r => new LookupDto { Id = r.SiteId, Title = r.SiteName ?? "" })
                    .ToListAsync());

        public Task<List<LookupDto>> GetStatusesAsync()
            => GetOrCreateAsync("cntb_statuses", () =>
                _context.Statuses.AsNoTracking()
                    .OrderBy(s => s.StatusId)
                    .Select(s => new LookupDto { Id = s.StatusId, Title = s.Title ?? "" })
                    .ToListAsync());

        public Task<List<LookupDto>> GetNhaCungUngAsync()
            => GetOrCreateAsync("cntb_nhacungung", () =>
                _context.NhaCungUngs.AsNoTracking()
                    .OrderBy(n => n.FullName)
                    .Select(n => new LookupDto { Id = n.CungUngId, Title = n.FullName ?? "" })
                    .ToListAsync());

        // ─── Cache helper ───
        private async Task<List<LookupDto>> GetOrCreateAsync(
            string cacheKey, Func<Task<List<LookupDto>>> factory)
        {
            if (_cache.TryGetValue(cacheKey, out List<LookupDto>? cached) && cached != null)
                return cached;

            var data = await factory();
            _cache.Set(cacheKey, data, CacheDuration);
            return data;
        }
    }
}
