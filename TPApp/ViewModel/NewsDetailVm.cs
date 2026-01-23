using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class NewsDetailVm
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public string? Author { get; set; }
        public string? PublishedDateText { get; set; }

        public List<Album> Images { get; set; } = new();
        public List<RelatedNewsVm> Related { get; set; } = new();
    }

    public class RelatedNewsVm
    {
        public long Id { get; set; }
        public int? MenuId { get; set; }
        public string? Title { get; set; }
        public string? QueryString { get; set; }
        public DateTime? PublishedDate { get; set; }
    }
}
