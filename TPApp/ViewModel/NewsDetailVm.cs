using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class NewsDetailVm
    {
        public long Id { get; set; }
        public int MenuId { get; set; }
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

    public class NewsCategoryVm
    {
        public int MenuId { get; set; }
        public string CategoryTitle { get; set; } = "";
        public List<NewsItemVm> Items { get; set; } = new();
        public PagerVm Pager { get; set; } = new();
    }

    public class NewsItemVm
    {
        public long Id { get; set; }
        public int? MenuId { get; set; }
        public string Title { get; set; } = "";
        public string QueryString { get; set; } = "";
        public string? Image { get; set; }
        public string? Description { get; set; }
        public DateTime? PublishedDate { get; set; }

        public string DetailUrl(string domain)
        {
            return $"{domain}{MenuId}/{QueryString}-{Id}.html";
        }

        public string PublishedDateText =>
            PublishedDate.HasValue
                ? PublishedDate.Value.ToString("MM/dd/yyyy")
                : "";
    }
}
