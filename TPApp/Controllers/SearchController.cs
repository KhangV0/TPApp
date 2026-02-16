using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TPApp.Application.DTOs;
using TPApp.Application.Helpers;
using TPApp.Application.Services;
using TPApp.Configuration;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    /// <summary>
    /// Controller for global search using centralized SearchIndexContents table
    /// Supports: Normal FullText search, AI semantic search, Autocomplete, Trending
    /// </summary>
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly IAISupplierMatchingService _aiMatchingService;
        private readonly ILogger<SearchController> _logger;
        private readonly FeatureFlags _featureFlags;

        public SearchController(
            ISearchService searchService,
            IAISupplierMatchingService aiMatchingService,
            ILogger<SearchController> logger,
            IOptions<FeatureFlags> featureFlags)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _aiMatchingService = aiMatchingService ?? throw new ArgumentNullException(nameof(aiMatchingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _featureFlags = featureFlags?.Value ?? new FeatureFlags();
        }

        /// <summary>
        /// Main search page with mode parameter (normal/ai)
        /// GET /search?q=keyword&mode=normal&page=1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string q, string mode = "normal", int page = 1)
        {
            var viewModel = new SearchViewModel
            {
                Query = q?.Trim() ?? string.Empty,
                Mode = mode?.ToLower() ?? "normal"
            };

            // Empty query - return empty view
            if (string.IsNullOrWhiteSpace(viewModel.Query))
            {
                _logger.LogDebug("Empty search query");
                return View(viewModel);
            }

            _logger.LogInformation("Search request: {Query}, Mode: {Mode}, Page: {Page}", 
                viewModel.Query, viewModel.Mode, page);

            try
            {
                var options = new SearchOptions
                {
                    PageNumber = page,
                    PageSize = 20,
                    TypeName = null // Get all types (no filter)
                };

                // Route to appropriate search mode
                if (viewModel.Mode == "ai" && _featureFlags.EnableAISearch == 1)
                {
                    await PerformAISearchAsync(viewModel, options);
                }
                else
                {
                    await PerformNormalSearchAsync(viewModel, options);
                }

                // Get trending searches for sidebar
                viewModel.TrendingSearches = await _searchService.GetTrendingSearchesAsync(7, 10);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search for query: {Query}", viewModel.Query);
                // Return partial results if available
            }

            // Pass feature flags to view
            ViewBag.EnableAISearch = _featureFlags.EnableAISearch;
            ViewBag.EnableKeywordSearch = 1; // Always enabled for new unified search
            ViewBag.MinAISearchLength = _featureFlags.MinAISearchLength;

            return View(viewModel);
        }

        /// <summary>
        /// Autocomplete suggestions endpoint
        /// GET /search/suggest?prefix=máy
        /// </summary>
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            {
                return Json(new List<SearchSuggestion>());
            }

            try
            {
                var suggestions = await _searchService.GetSuggestionsAsync(prefix);
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggestions for prefix: {Prefix}", prefix);
                return Json(new List<SearchSuggestion>());
            }
        }

        /// <summary>
        /// Trending searches endpoint
        /// GET /search/trending?days=7&topN=10
        /// </summary>
        [HttpGet("trending")]
        public async Task<IActionResult> Trending(int days = 7, int topN = 10)
        {
            try
            {
                var trending = await _searchService.GetTrendingSearchesAsync(days, topN);
                return Json(trending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending searches");
                return Json(new List<TrendingSearch>());
            }
        }

        // =============================================
        // Private Helper Methods
        // =============================================

        /// <summary>
        /// Perform normal FullText search
        /// </summary>
        private async Task PerformNormalSearchAsync(SearchViewModel viewModel, SearchOptions options)
        {
            _logger.LogDebug("Performing normal FullText search for: {Query}", viewModel.Query);

            var result = await _searchService.SearchNormalAsync(viewModel.Query, options);

            // Convert SearchIndexContent to view models
            viewModel.SearchResults = result.Items.Select(item => new SearchResultItem
            {
                Id = item.RefId ?? 0,
                Title = SearchHighlightHelper.HighlightKeywords(item.Title ?? string.Empty, viewModel.Query),
                Description = SearchHighlightHelper.CreateSnippet(
                    item.Description ?? item.Contents ?? string.Empty, 
                    viewModel.Query, 
                    200),
                Url = item.URL ?? string.Empty,
                ImageUrl = item.ImgPreview ?? string.Empty,
                TypeName = item.TypeName ?? string.Empty,
                Created = item.Created
            }).ToList();

            viewModel.TotalResults = result.TotalCount;
            viewModel.CurrentPage = result.PageNumber;
            viewModel.TotalPages = result.TotalPages;

            _logger.LogInformation("Normal search found {Count} results", result.TotalCount);
        }

        /// <summary>
        /// Perform AI semantic search
        /// </summary>
        private async Task PerformAISearchAsync(SearchViewModel viewModel, SearchOptions options)
        {
            // Check minimum length requirement
            if (viewModel.Query.Length < _featureFlags.MinAISearchLength)
            {
                _logger.LogDebug("Query too short for AI search ({Length} chars), minimum is {Min}. Falling back to normal search.",
                    viewModel.Query.Length, _featureFlags.MinAISearchLength);
                
                await PerformNormalSearchAsync(viewModel, options);
                return;
            }

            _logger.LogDebug("Performing AI semantic search for: {Query}", viewModel.Query);

            // Get grouped AI results
            viewModel.AISearchResults = await _searchService.SearchAIGroupedAsync(viewModel.Query, options);

            viewModel.TotalResults = viewModel.AISearchResults.Sum(g => g.Products.Count);
            viewModel.CurrentPage = options.PageNumber;
            viewModel.TotalPages = (int)Math.Ceiling(viewModel.TotalResults / (double)options.PageSize);

            _logger.LogInformation("AI search found {Companies} companies with {Total} products", 
                viewModel.AISearchResults.Count,
                viewModel.TotalResults);
        }
    }
}
