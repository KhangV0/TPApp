using TPApp.Application.DTOs;

namespace TPApp.ViewModel
{
    /// <summary>
    /// Entity type for search results filtering
    /// </summary>
    public enum SearchEntityType
    {
        All = 0,
        Technology = 1,
        Equipment = 2,
        IntellectualProperty = 3,
        Supplier = 4,
        Expert = 5,
        Article = 6
    }

    /// <summary>
    /// Mapping helper between DB TypeName strings and SearchEntityType enum
    /// </summary>
    public static class SearchEntityTypeHelper
    {
        // ── Exact DB TypeName values ──
        private static readonly Dictionary<string, SearchEntityType> NameToEnum = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Công nghệ", SearchEntityType.Technology },
            { "Thiết bị", SearchEntityType.Equipment },
            { "Tài sản trí tuệ", SearchEntityType.IntellectualProperty },
            { "Nhà cung ứng", SearchEntityType.Supplier },
            { "Chuyên gia", SearchEntityType.Expert },
            { "Tin bài", SearchEntityType.Article },
        };

        // ── Must match DB values exactly for SQL WHERE clause ──
        private static readonly Dictionary<SearchEntityType, string> EnumToName = new()
        {
            { SearchEntityType.Technology, "Công nghệ" },
            { SearchEntityType.Equipment, "Thiết bị" },
            { SearchEntityType.IntellectualProperty, "Tài sản trí tuệ" },
            { SearchEntityType.Supplier, "Nhà cung ứng" },
            { SearchEntityType.Expert, "Chuyên gia" },
            { SearchEntityType.Article, "Tin bài" },
        };

        private static readonly Dictionary<SearchEntityType, string> EnumToLabel = new()
        {
            { SearchEntityType.All, "Tất cả" },
            { SearchEntityType.Technology, "Công nghệ" },
            { SearchEntityType.Equipment, "Thiết bị" },
            { SearchEntityType.IntellectualProperty, "Tài sản trí tuệ" },
            { SearchEntityType.Supplier, "Nhà cung ứng" },
            { SearchEntityType.Expert, "Chuyên gia" },
            { SearchEntityType.Article, "Tin bài" },
        };

        private static readonly Dictionary<SearchEntityType, string> EnumToIcon = new()
        {
            { SearchEntityType.All, "bi-grid" },
            { SearchEntityType.Technology, "bi-cpu" },
            { SearchEntityType.Equipment, "bi-gear" },
            { SearchEntityType.IntellectualProperty, "bi-lightbulb" },
            { SearchEntityType.Supplier, "bi-building" },
            { SearchEntityType.Expert, "bi-person-badge" },
            { SearchEntityType.Article, "bi-newspaper" },
        };

        /// <summary>
        /// Map DB TypeName → enum. Unknown types default to Technology.
        /// </summary>
        public static SearchEntityType FromTypeName(string? typeName) =>
            !string.IsNullOrEmpty(typeName) && NameToEnum.TryGetValue(typeName, out var t)
                ? t : SearchEntityType.Technology;

        public static string? ToTypeName(SearchEntityType type) =>
            EnumToName.TryGetValue(type, out var n) ? n : null;

        public static string ToLabel(SearchEntityType type) =>
            EnumToLabel.TryGetValue(type, out var l) ? l : type.ToString();

        public static string ToIcon(SearchEntityType type) =>
            EnumToIcon.TryGetValue(type, out var i) ? i : "bi-file-earmark";
    }

    /// <summary>
    /// Individual result item for enterprise search card
    /// </summary>
    public class SearchResultItemVm
    {
        public SearchEntityType EntityType { get; set; }
        public long EntityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SnippetHtml { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, string> Meta { get; set; } = new();
        public string? ContactUrl { get; set; }
        public string? SaveUrl { get; set; }
    }

    /// <summary>
    /// Page-level view model for enterprise search results
    /// </summary>
    public class SearchResultPageVm
    {
        public string Query { get; set; } = string.Empty;
        public SearchEntityType Type { get; set; } = SearchEntityType.All;
        public string Sort { get; set; } = "relevance";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int Total { get; set; }
        public Dictionary<SearchEntityType, int> CountsByType { get; set; } = new();
        public List<SearchResultItemVm> Items { get; set; } = new();

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(Total / (double)PageSize) : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        // AI tab support (keep compatibility)
        public string Mode { get; set; } = "normal";
        public int AIResultCount { get; set; }
        public List<AISearchResultGroup> AISearchResults { get; set; } = new();
    }
}
