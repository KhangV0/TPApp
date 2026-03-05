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
using TPApp.Data.Entities;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    /// <summary>
    /// Enterprise search controller – supports Normal FullText + AI semantic search,
    /// type filtering, sorting, AJAX partial loading, and pagination.
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

        // ─── Full page ────────────────────────────────────────────────
        // GET /Search?q=keyword&mode=normal&type=all&sort=relevance&page=1
        [HttpGet]
        public async Task<IActionResult> Index(
            string q, string mode = "normal", string type = "all",
            string sort = "relevance", int page = 1)
        {
            var vm = BuildVm(q, mode, type, sort, page);

            if (string.IsNullOrWhiteSpace(vm.Query))
                return View(vm);

            _logger.LogInformation(
                "Search: q={Query} mode={Mode} type={Type} sort={Sort} page={Page}",
                vm.Query, vm.Mode, vm.Type, vm.Sort, vm.Page);

            try
            {
                // Counts for tabs (always run on the full keyword, no type filter)
                var rawCounts = await _searchService.GetCountsByTypeAsync(vm.Query);
                vm.CountsByType = MapCounts(rawCounts);

                // Normal search results
                if (vm.Mode != "ai")
                {
                    try
                    {
                        await FillNormalResultsAsync(vm);
                    }
                    catch (Exception normalEx)
                    {
                        _logger.LogError(normalEx, "NORMAL SEARCH ERROR: {Message}", normalEx.Message);
                    }
                }

                // AI search (if enabled and selected)
                if (_featureFlags.EnableAISearch == 1)
                {
                    if (vm.Mode == "ai")
                    {
                        try
                        {
                            var aiOpts = new SearchOptions { PageNumber = 1, PageSize = 100 };
                            vm.AISearchResults = await _searchService.SearchAIGroupedAsync(vm.Query, aiOpts);
                            vm.AIResultCount = vm.AISearchResults.Sum(g => g.Products.Count);
                        }
                        catch (Exception aiEx)
                        {
                            _logger.LogWarning(aiEx, "AI search failed, fallback to normal");
                        }
                    }
                    else
                    {
                        // Quick AI count for the tab badge (use same method as AI tab)
                        try
                        {
                            var aiOpts = new SearchOptions { PageNumber = 1, PageSize = 100 };
                            var aiGroups = await _searchService.SearchAIGroupedAsync(vm.Query, aiOpts);
                            vm.AIResultCount = aiGroups.Sum(g => g.Products.Count);
                        }
                        catch { /* non-critical */ }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search error for: {Query}", vm.Query);
            }

            ViewBag.EnableAISearch = _featureFlags.EnableAISearch;
            return View(vm);
        }

        // ─── AJAX partial ────────────────────────────────────────────
        // GET /Search/ResultsPartial?q=keyword&type=Technology&sort=relevance&page=1
        [HttpGet]
        public async Task<IActionResult> ResultsPartial(
            string q, string type = "all", string sort = "relevance", int page = 1)
        {
            var vm = BuildVm(q, "normal", type, sort, page);

            if (!string.IsNullOrWhiteSpace(vm.Query))
            {
                try
                {
                    var rawCounts = await _searchService.GetCountsByTypeAsync(vm.Query);
                    vm.CountsByType = MapCounts(rawCounts);
                    await FillNormalResultsAsync(vm);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ResultsPartial error for: {Query}", vm.Query);
                }
            }

            return PartialView("_SearchResults", vm);
        }

        // ─── Autocomplete ────────────────────────────────────────────
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
                return Json(new List<SearchSuggestion>());

            try
            {
                var suggestions = await _searchService.GetSuggestionsAsync(prefix);
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Suggest error: {Prefix}", prefix);
                return Json(new List<SearchSuggestion>());
            }
        }

        // ─── Trending ────────────────────────────────────────────────
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
                _logger.LogError(ex, "Trending error");
                return Json(new List<TrendingSearch>());
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Private helpers
        // ═══════════════════════════════════════════════════════════

        private SearchResultPageVm BuildVm(string q, string mode, string type, string sort, int page)
        {
            return new SearchResultPageVm
            {
                Query = q?.Trim() ?? string.Empty,
                Mode = mode?.ToLower() ?? "normal",
                Type = ParseEntityType(type),
                Sort = sort ?? "relevance",
                Page = Math.Max(1, page),
                PageSize = 20
            };
        }

        /// <summary>
        /// Execute normal FullText search using inline CONTAINSTABLE with type filter.
        /// </summary>
        private async Task FillNormalResultsAsync(SearchResultPageVm vm)
        {
            var options = new SearchOptions
            {
                PageNumber = vm.Page,
                PageSize = vm.PageSize,
                TypeName = SearchEntityTypeHelper.ToTypeName(vm.Type) // null for "All"
            };

            var result = await _searchService.SearchByTypeAsync(vm.Query, options);

            vm.Items = result.Items.Select(item => MapItem(item, vm.Query)).ToList();
            vm.Total = result.TotalCount;
        }

        private static SearchResultItemVm MapItem(SearchIndexContent item, string query)
        {
            var entityType = SearchEntityTypeHelper.FromTypeName(item.TypeName);

            return new SearchResultItemVm
            {
                EntityType = entityType,
                EntityId = item.RefId ?? item.Id,
                Title = SearchHighlightHelper.HighlightKeywords(item.Title ?? string.Empty, query),
                SnippetHtml = SearchHighlightHelper.CreateSnippet(
                    item.Description ?? item.Contents ?? string.Empty, query, 250),
                Url = item.URL ?? string.Empty,
                UpdatedDate = item.Modified ?? item.Created,
                Tags = new List<string> { SearchEntityTypeHelper.ToLabel(entityType) }
            };
        
        }

        private static SearchEntityType ParseEntityType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type) || type.Equals("all", StringComparison.OrdinalIgnoreCase))
                return SearchEntityType.All;
            return Enum.TryParse<SearchEntityType>(type, true, out var result)
                ? result : SearchEntityType.All;
        }

        /// <summary>
        /// Convert raw TypeName→Count dict to SearchEntityType→Count dict
        /// </summary>
        private static Dictionary<SearchEntityType, int> MapCounts(Dictionary<string, int> raw)
        {
            var result = new Dictionary<SearchEntityType, int>();
            int total = 0;
            foreach (var kvp in raw)
            {
                var et = SearchEntityTypeHelper.FromTypeName(kvp.Key);
                if (result.ContainsKey(et))
                    result[et] += kvp.Value;
                else
                    result[et] = kvp.Value;
                total += kvp.Value;
            }
            result[SearchEntityType.All] = total;
            return result;
        }
    }
}
