using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Enums;

namespace TPApp.Services
{
    public interface IEntityOwnershipResolver
    {
        Task<bool> IsOwnerAsync(string entityType, int entityId, string userId);
    }

    public class EntityOwnershipResolver : IEntityOwnershipResolver
    {
        private readonly AppDbContext _context;

        public EntityOwnershipResolver(AppDbContext context) => _context = context;

        public async Task<bool> IsOwnerAsync(string entityType, int entityId, string userId)
        {
            if (!int.TryParse(userId, out int uid)) return false;

            return entityType switch
            {
                EntityTypes.SanPhamCNTB => await IsSanPhamOwnerAsync(entityId, uid, userId),
                EntityTypes.NhaCungUng  => await IsNhaCungUngOwnerAsync(entityId, uid),
                EntityTypes.NhaTuVan    => false, // NhaTuVan typically has no single owner
                EntityTypes.Contents    => await IsContentsOwnerAsync(entityId, userId),
                _ => false
            };
        }

        private async Task<bool> IsSanPhamOwnerAsync(int id, int uid, string userIdStr)
        {
            var product = await _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(p => p.ID == id)
                .Select(p => new { p.Creator, p.OwnerEmail, p.NCUId })
                .FirstOrDefaultAsync();

            if (product == null) return false;

            // Check Creator username match
            var userName = await _context.Users.AsNoTracking()
                .Where(u => u.Id == uid).Select(u => u.UserName).FirstOrDefaultAsync();
            if (userName != null && userName == product.Creator) return true;

            // Check NCUId → NhaCungUng.UserId
            if (product.NCUId.HasValue && product.NCUId > 0)
            {
                var ncuUserId = await _context.NhaCungUngs.AsNoTracking()
                    .Where(n => n.CungUngId == product.NCUId)
                    .Select(n => n.UserId)
                    .FirstOrDefaultAsync();
                if (ncuUserId == uid) return true;
            }

            return false;
        }

        private async Task<bool> IsNhaCungUngOwnerAsync(int id, int uid)
        {
            return await _context.NhaCungUngs.AsNoTracking()
                .AnyAsync(n => n.CungUngId == id && n.UserId == uid);
        }

        private async Task<bool> IsContentsOwnerAsync(int id, string userId)
        {
            var creator = await _context.Contents.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => c.Creator)
                .FirstOrDefaultAsync();

            if (creator == null) return false;
            if (int.TryParse(userId, out int uid))
            {
                var userName = await _context.Users.AsNoTracking()
                    .Where(u => u.Id == uid).Select(u => u.UserName).FirstOrDefaultAsync();
                return userName != null && userName == creator;
            }
            return false;
        }
    }
}
