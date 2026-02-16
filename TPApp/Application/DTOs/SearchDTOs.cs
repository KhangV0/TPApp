using TPApp.Data.Entities;

namespace TPApp.Application.DTOs
{
    /// <summary>
    /// Search options for filtering and pagination
    /// </summary>
    public class SearchOptions
    {
        public string? LanguageId { get; set; }
        public string? TypeName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Search result with pagination info
    /// </summary>
    public class SearchResult
    {
        public List<SearchIndexContent> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Autocomplete suggestion
    /// </summary>
    public class SearchSuggestion
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TypeName { get; set; }
        public string? URL { get; set; }
        public string? ImgPreview { get; set; }
        public int Rank { get; set; }
    }

    /// <summary>
    /// Trending search keyword
    /// </summary>
    public class TrendingSearch
    {
        public string NormalizedKeyword { get; set; } = string.Empty;
        public int SearchCount { get; set; }
        public int AvgResults { get; set; }
        public DateTime LastSearched { get; set; }
        public string SearchModes { get; set; } = string.Empty;
    }
}
