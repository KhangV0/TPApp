using TPApp.Application.DTOs;

namespace TPApp.Application.Services
{
    /// <summary>
    /// Search service interface for normal and AI search
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Perform normal FullText search
        /// </summary>
        Task<SearchResult> SearchNormalAsync(
            string keyword, 
            SearchOptions options, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform AI-enhanced semantic search
        /// </summary>
        Task<SearchResult> SearchAIAsync(
            string keyword, 
            SearchOptions options, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform AI-enhanced semantic search with results grouped by company/organization
        /// </summary>
        Task<List<ViewModel.AISearchResultGroup>> SearchAIGroupedAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get autocomplete suggestions
        /// </summary>
        Task<List<SearchSuggestion>> GetSuggestionsAsync(
            string prefix, 
            string? languageId = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get trending searches
        /// </summary>
        Task<List<TrendingSearch>> GetTrendingSearchesAsync(
            int days = 7, 
            int topN = 10, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get result counts grouped by TypeName for filter tabs
        /// </summary>
        Task<Dictionary<string, int>> GetCountsByTypeAsync(
            string keyword,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search with TypeName filter using inline SQL (bypasses SP).
        /// </summary>
        Task<SearchResult> SearchByTypeAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken = default);
    }
}
