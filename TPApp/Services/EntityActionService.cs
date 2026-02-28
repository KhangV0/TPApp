using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class EntityActionService : IEntityActionService
    {
        private readonly AppDbContext _context;
        private readonly IEntityOwnershipResolver _ownership;

        public EntityActionService(AppDbContext context, IEntityOwnershipResolver ownership)
        {
            _context = context;
            _ownership = ownership;
        }

        // ── Get Summary ─────────────────────────────────────────────────────
        public async Task<EntitySummaryVm> GetSummaryAsync(string entityType, int entityId, string? userId)
        {
            var ratings = await _context.EntityRatings
                .AsNoTracking()
                .Where(r => r.EntityType == entityType && r.EntityId == entityId && r.StatusId == 1)
                .ToListAsync();

            var vm = new EntitySummaryVm
            {
                EntityType = entityType,
                EntityId = entityId,
                TotalRatings = ratings.Count,
                AverageStars = ratings.Count > 0 ? Math.Round(ratings.Average(r => r.Stars), 1) : 0,
                StarDistribution = new int[5]
            };

            // Star distribution
            foreach (var r in ratings)
                if (r.Stars >= 1 && r.Stars <= 5)
                    vm.StarDistribution[r.Stars - 1]++;

            // View count
            vm.TotalViews = await GetViewCountAsync(entityType, entityId);

            // User's existing rating
            if (!string.IsNullOrEmpty(userId))
            {
                var userRating = ratings.FirstOrDefault(r => r.UserId == userId);
                if (userRating != null)
                {
                    vm.UserRating = new UserRatingVm
                    {
                        Stars = userRating.Stars,
                        Comment = userRating.Comment
                    };
                }

                vm.IsOwner = await _ownership.IsOwnerAsync(entityType, entityId, userId);
            }

            return vm;
        }

        // ── Save Rating ─────────────────────────────────────────────────────
        public async Task<SaveRatingResponse> SaveRatingAsync(
            string entityType, int entityId, string userId, SaveRatingRequest request)
        {
            if (request.Stars < 1 || request.Stars > 5)
                return new SaveRatingResponse { Error = "Đánh giá phải từ 1 đến 5 sao." };

            // Owner check
            if (await _ownership.IsOwnerAsync(entityType, entityId, userId))
                return new SaveRatingResponse { Error = "Bạn không thể đánh giá nội dung của mình." };

            // Find existing or create
            var existing = await _context.EntityRatings
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.EntityType == entityType &&
                    r.EntityId == entityId);

            if (existing != null)
            {
                existing.Stars = request.Stars;
                existing.Title = request.Title;
                existing.Comment = request.Comment;
                existing.Modified = DateTime.UtcNow;
            }
            else
            {
                _context.EntityRatings.Add(new EntityRating
                {
                    EntityId = entityId,
                    EntityType = entityType,
                    UserId = userId,
                    Stars = request.Stars,
                    Title = request.Title,
                    Comment = request.Comment,
                    StatusId = 1,
                    Created = DateTime.UtcNow
                });
            }

            // Log action
            _context.EntityActionLogs.Add(new EntityActionLog
            {
                EntityId = entityId,
                EntityType = entityType,
                UserId = userId,
                ActionType = ActionTypes.Rate,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { request.Stars }),
                Created = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Recalculate average
            var stats = await _context.EntityRatings
                .AsNoTracking()
                .Where(r => r.EntityType == entityType && r.EntityId == entityId && r.StatusId == 1)
                .GroupBy(r => 1)
                .Select(g => new { Avg = g.Average(r => r.Stars), Count = g.Count() })
                .FirstOrDefaultAsync();

            // Update entity's own Rating column if applicable
            await UpdateEntityRatingColumnAsync(entityType, entityId, stats?.Avg);

            return new SaveRatingResponse
            {
                Success = true,
                NewAverage = Math.Round(stats?.Avg ?? 0, 1),
                TotalRatings = stats?.Count ?? 0
            };
        }

        // ── Increase View ───────────────────────────────────────────────────
        public async Task IncreaseViewAsync(string entityType, int entityId, string? userId, string? ipAddress)
        {
            // Log action
            _context.EntityActionLogs.Add(new EntityActionLog
            {
                EntityId = entityId,
                EntityType = entityType,
                UserId = userId,
                ActionType = ActionTypes.View,
                MetadataJson = ipAddress != null
                    ? System.Text.Json.JsonSerializer.Serialize(new { ip = ipAddress })
                    : null,
                Created = DateTime.UtcNow
            });

            // Update view count on entity or fallback table
            bool updated = entityType switch
            {
                EntityTypes.SanPhamCNTB => await IncrementSanPhamViewAsync(entityId),
                _ => false
            };

            if (!updated)
            {
                // Fallback: EntityViewCounters
                var counter = await _context.EntityViewCounters
                    .FirstOrDefaultAsync(v => v.EntityType == entityType && v.EntityId == entityId);

                if (counter != null)
                {
                    counter.ViewCount++;
                }
                else
                {
                    _context.EntityViewCounters.Add(new EntityViewCounter
                    {
                        EntityType = entityType,
                        EntityId = entityId,
                        ViewCount = 1
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // ── Get Ratings List ────────────────────────────────────────────────
        public async Task<List<RatingItemVm>> GetRatingsAsync(string entityType, int entityId, int take = 20)
        {
            var ratings = await _context.EntityRatings
                .AsNoTracking()
                .Where(r => r.EntityType == entityType && r.EntityId == entityId && r.StatusId == 1)
                .OrderByDescending(r => r.Created)
                .Take(take)
                .ToListAsync();

            var result = new List<RatingItemVm>();
            foreach (var r in ratings)
            {
                string userName = "Người dùng";
                if (int.TryParse(r.UserId, out int uid))
                {
                    userName = await _context.Users.AsNoTracking()
                        .Where(u => u.Id == uid)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync() ?? "Người dùng";
                }

                result.Add(new RatingItemVm
                {
                    Id = r.Id,
                    UserName = userName,
                    Stars = r.Stars,
                    Comment = r.Comment,
                    Created = r.Created
                });
            }
            return result;
        }

        // ── Private Helpers ─────────────────────────────────────────────────

        private async Task<int> GetViewCountAsync(string entityType, int entityId)
        {
            if (entityType == EntityTypes.SanPhamCNTB)
            {
                return await _context.SanPhamCNTBs.AsNoTracking()
                    .Where(p => p.ID == entityId)
                    .Select(p => p.Viewed ?? 0)
                    .FirstOrDefaultAsync();
            }

            // Fallback
            return await _context.EntityViewCounters.AsNoTracking()
                .Where(v => v.EntityType == entityType && v.EntityId == entityId)
                .Select(v => v.ViewCount)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> IncrementSanPhamViewAsync(int id)
        {
            var product = await _context.SanPhamCNTBs.FirstOrDefaultAsync(p => p.ID == id);
            if (product == null) return false;
            product.Viewed = (product.Viewed ?? 0) + 1;
            return true;
        }

        private async Task UpdateEntityRatingColumnAsync(string entityType, int entityId, double? avg)
        {
            if (entityType == EntityTypes.SanPhamCNTB && avg.HasValue)
            {
                var product = await _context.SanPhamCNTBs.FirstOrDefaultAsync(p => p.ID == entityId);
                if (product != null)
                    product.Rating = (int)Math.Round(avg.Value);
                await _context.SaveChangesAsync();
            }
        }
    }
}
