using TPApp.Application.DTOs;
using TPApp.Domain.Models;

namespace TPApp.ViewModel
{
    /// <summary>
    /// View model for unified search page
    /// Supports both Normal FullText search and AI semantic search
    /// </summary>
    public class SearchViewModel
    {
        /// <summary>
        /// The search query entered by user
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Search mode: 'normal' or 'ai'
        /// </summary>
        public string Mode { get; set; } = "normal";

        /// <summary>
        /// Unified search results from SearchIndexContents
        /// </summary>
        public List<SearchResultItem> SearchResults { get; set; } = new List<SearchResultItem>();

        /// <summary>
        /// AI search results grouped by company/organization
        /// </summary>
        public List<AISearchResultGroup> AISearchResults { get; set; } = new List<AISearchResultGroup>();

        /// <summary>
        /// Total number of results found (for pagination of active mode)
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// Count of normal search results
        /// </summary>
        public int NormalResultCount { get; set; }

        /// <summary>
        /// Count of AI search results
        /// </summary>
        public int AIResultCount { get; set; }

        /// <summary>
        /// Current page number (1-indexed)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Trending searches for sidebar
        /// </summary>
        public List<TrendingSearch> TrendingSearches { get; set; } = new List<TrendingSearch>();

        /// <summary>
        /// Whether there are more pages
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        // =============================================
        // Legacy properties for backward compatibility
        // =============================================

        /// <summary>
        /// AI-matched suppliers with scores (Tab 1) - LEGACY
        /// </summary>
        public List<SupplierMatchResult> AiSuppliers { get; set; } = new List<SupplierMatchResult>();

        /// <summary>
        /// Keyword-matched products (Tab 2) - LEGACY
        /// </summary>
        public List<ProductSearchItem> Products { get; set; } = new List<ProductSearchItem>();

        /// <summary>
        /// Keyword-matched suppliers (Tab 2) - LEGACY
        /// </summary>
        public List<SupplierSearchItem> Suppliers { get; set; } = new List<SupplierSearchItem>();

        /// <summary>
        /// Whether AI search is enabled (query length >= 5) - LEGACY
        /// </summary>
        public bool IsAiSearchEnabled => !string.IsNullOrWhiteSpace(Query) && Query.Length >= 5;
    }

    /// <summary>
    /// Individual search result item
    /// </summary>
    public class SearchResultItem
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public DateTime? Created { get; set; }
    }

    /// <summary>
    /// Product item for keyword search results - LEGACY
    /// </summary>
    public class ProductSearchItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public DateTime? Created { get; set; }
    }

    /// <summary>
    /// Supplier item for keyword search results - LEGACY
    /// </summary>
    public class SupplierSearchItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int ProductCount { get; set; }
        public int Viewed { get; set; }
    }
}
