namespace TPApp.Interfaces
{
    public interface IEntityActionService
    {
        /// <summary>Get rating summary + user's existing rating for an entity.</summary>
        Task<EntitySummaryVm> GetSummaryAsync(string entityType, int entityId, string? userId);

        /// <summary>Save or update a rating.</summary>
        Task<SaveRatingResponse> SaveRatingAsync(string entityType, int entityId, string userId, SaveRatingRequest request);

        /// <summary>Increase view count (with anti-spam).</summary>
        Task IncreaseViewAsync(string entityType, int entityId, string? userId, string? ipAddress);

        /// <summary>Get recent ratings for an entity.</summary>
        Task<List<RatingItemVm>> GetRatingsAsync(string entityType, int entityId, int take = 20);
    }

    // ── ViewModels ──────────────────────────────────────────────────────────

    public class EntitySummaryVm
    {
        public string EntityType { get; set; } = "";
        public int EntityId { get; set; }
        public double AverageStars { get; set; }
        public int TotalRatings { get; set; }
        public int TotalViews { get; set; }
        public int[] StarDistribution { get; set; } = new int[5]; // [0]=1star, [4]=5star
        public UserRatingVm? UserRating { get; set; }
        public bool IsOwner { get; set; }
    }

    public class UserRatingVm
    {
        public int Stars { get; set; }
        public string? Comment { get; set; }
    }

    public class RatingItemVm
    {
        public long Id { get; set; }
        public string UserName { get; set; } = "";
        public int Stars { get; set; }
        public string? Comment { get; set; }
        public DateTime Created { get; set; }
    }

    public class SaveRatingRequest
    {
        public int Stars { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
    }

    public class SaveRatingResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public double NewAverage { get; set; }
        public int TotalRatings { get; set; }
    }
}
