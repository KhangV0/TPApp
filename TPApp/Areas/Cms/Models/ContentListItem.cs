namespace TPApp.Areas.Cms.Models
{
    /// <summary>
    /// Projection DTO for Content list view (avoid loading HTML body).
    /// </summary>
    public class ContentListItem
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string? MenuTitle { get; set; }
        public int? MenuId { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public int Viewed { get; set; }
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }
        public string? Author { get; set; }
        public string? Image { get; set; }
        public string? Creator { get; set; }
        public int? SiteId { get; set; }
    }
}
