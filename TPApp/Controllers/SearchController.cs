using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TPApp.Application.Services;
using TPApp.Configuration;
using TPApp.Data;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    /// <summary>
    /// Controller for global search with AI and keyword tabs
    /// </summary>
    public class SearchController : Controller
    {
        private readonly IAISupplierMatchingService _aiMatchingService;
        private readonly AppDbContext _context;
        private readonly ILogger<SearchController> _logger;
        private readonly FeatureFlags _featureFlags;

        private const int MAX_PRODUCTS = 50;
        private const int MAX_SUPPLIERS = 20;

        public SearchController(
            IAISupplierMatchingService aiMatchingService,
            AppDbContext context,
            ILogger<SearchController> logger,
            IOptions<FeatureFlags> featureFlags)
        {
            _aiMatchingService = aiMatchingService ?? throw new ArgumentNullException(nameof(aiMatchingService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _featureFlags = featureFlags?.Value ?? new FeatureFlags();
        }

        /// <summary>
        /// Search results page with AI and keyword tabs
        /// </summary>
        /// <param name="q">Search query</param>
        [HttpGet]
        public async Task<IActionResult> Index(string q)
        {
            var viewModel = new SearchViewModel
            {
                Query = q?.Trim() ?? string.Empty
            };

            // Empty query - return empty view
            if (string.IsNullOrWhiteSpace(viewModel.Query))
            {
                _logger.LogDebug("Empty search query");
                return View(viewModel);
            }

            _logger.LogInformation("Search request: {Query}", viewModel.Query);

            try
            {
                // AI Search (if enabled and query long enough)
                if (_featureFlags.EnableAISearch == 1 && 
                    !string.IsNullOrWhiteSpace(viewModel.Query) &&
                    viewModel.Query.Length >= _featureFlags.MinAISearchLength)
                {
                    _logger.LogDebug("Performing AI search for: {Query}", viewModel.Query);
                    var aiResults = await _aiMatchingService.FindMatchingSuppliersAsync(viewModel.Query);
                    
                    // Apply max results limit
                    viewModel.AiSuppliers = aiResults.Take(_featureFlags.MaxAISearchResults).ToList();
                    _logger.LogInformation("AI search found {Count} suppliers (limited to {Max})", 
                        aiResults.Count, _featureFlags.MaxAISearchResults);
                }
                else
                {
                    if (_featureFlags.EnableAISearch == 0)
                    {
                        _logger.LogDebug("AI search disabled by feature flag");
                    }
                    else
                    {
                        _logger.LogDebug("Query too short for AI search ({Length} chars), minimum is {Min}",
                            viewModel.Query.Length, _featureFlags.MinAISearchLength);
                    }
                }

                // Keyword Search (if enabled) - Run in parallel for better performance
                if (_featureFlags.EnableKeywordSearch == 1)
                {
                    var productsTask = SearchProductsAsync(viewModel.Query);
                    var suppliersTask = SearchSuppliersAsync(viewModel.Query);

                    await Task.WhenAll(productsTask, suppliersTask);

                    viewModel.Products = productsTask.Result;
                    viewModel.Suppliers = suppliersTask.Result;

                    _logger.LogInformation("Keyword search found {ProductCount} products, {SupplierCount} suppliers", 
                        viewModel.Products.Count, viewModel.Suppliers.Count);
                }
                else
                {
                    _logger.LogDebug("Keyword search disabled by feature flag");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search for query: {Query}", viewModel.Query);
                // Return partial results if available
            }

            // Pass feature flags to view for conditional rendering
            ViewBag.EnableAISearch = _featureFlags.EnableAISearch;
            ViewBag.EnableKeywordSearch = _featureFlags.EnableKeywordSearch;
            ViewBag.MinAISearchLength = _featureFlags.MinAISearchLength;

            return View(viewModel);
        }

        /// <summary>
        /// Search products by keyword (Name, Keywords)
        /// Optimized: Uses EF.Functions.Like for better performance, removed MoTa search
        /// </summary>
        private async Task<List<ProductSearchItem>> SearchProductsAsync(string query)
        {
            var searchPattern = $"%{query}%";

            var products = await _context.SanPhamCNTBs
                .Where(p => p.StatusId == 3 && // Only published products
                           (EF.Functions.Like(p.Name, searchPattern) ||
                            (p.Keywords != null && EF.Functions.Like(p.Keywords, searchPattern))))
                .OrderByDescending(p => p.Created)
                .Take(MAX_PRODUCTS)
                .Select(p => new ProductSearchItem
                {
                    Id = p.ID,
                    Name = p.Name,
                    Url = p.URL ?? string.Empty,
                    CategoryName = null, // Category navigation not available
                    Created = p.Created
                })
                .ToListAsync();

            return products;
        }

        /// <summary>
        /// Search suppliers by keyword (FullName, Keywords)
        /// Optimized: Uses EF.Functions.Like and single query with JOIN for product counts
        /// </summary>
        private async Task<List<SupplierSearchItem>> SearchSuppliersAsync(string query)
        {
            var searchPattern = $"%{query}%";

            // Single query with LEFT JOIN to get product counts
            var results = await (
                from s in _context.NhaCungUngs
                where (s.FullName != null && EF.Functions.Like(s.FullName, searchPattern)) ||
                      (s.Keywords != null && EF.Functions.Like(s.Keywords, searchPattern))
                join p in _context.SanPhamCNTBs.Where(p => p.StatusId == 3)
                    on s.CungUngId equals p.NCUId into products
                select new SupplierSearchItem
                {
                    Id = s.CungUngId,
                    FullName = s.FullName ?? string.Empty,
                    Rating = s.Rating ?? 0,
                    ProductCount = products.Count(),
                    Viewed = s.Viewed ?? 0
                })
                .OrderByDescending(s => s.Viewed)
                .Take(MAX_SUPPLIERS)
                .ToListAsync();

            return results;
        }
    }
}
