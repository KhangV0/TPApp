using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TPApp.Application.DTOs;
using TPApp.Data;
using TPApp.Data.Entities;

namespace TPApp.Application.Services
{
    /// <summary>
    /// Search service implementation using FullText Search on SearchIndexContents
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<SearchService> _logger;
        private readonly IConfiguration _configuration;

        // Cache TTL settings
        private const int NORMAL_SEARCH_CACHE_SECONDS = 120;
        private const int AI_SEARCH_CACHE_SECONDS = 300;
        private const int SUGGEST_CACHE_SECONDS = 600;
        private const int TRENDING_CACHE_SECONDS = 300;

        public SearchService(
            AppDbContext context,
            IDistributedCache cache,
            ILogger<SearchService> logger,
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<SearchResult> SearchNormalAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    _logger.LogWarning("Empty keyword provided to SearchNormalAsync");
                    return new SearchResult();
                }

                // Normalize keyword for accent-insensitive search
                var originalKeyword = keyword;
                keyword = TPApp.Helpers.VietnameseTextHelper.NormalizeKeyword(keyword);
                
                if (originalKeyword != keyword)
                {
                    _logger.LogDebug("Keyword normalized: '{Original}' → '{Normalized}'", originalKeyword, keyword);
                }

                // Check cache first - DISABLED FOR DEBUGGING
                // var cacheKey = BuildCacheKey("normal", keyword, options);
                // var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

                // if (cached != null)
                // {
                //     _logger.LogDebug("Cache hit for normal search: {Keyword}", keyword);
                //     return JsonSerializer.Deserialize<SearchResult>(cached)!;
                // }

                _logger.LogInformation("Executing normal search for: {Keyword} (CACHE DISABLED)", keyword);

                // Execute stored procedure
                var keywordParam = new SqlParameter("@Keyword", keyword);
                var languageParam = new SqlParameter("@LanguageId", (object?)options.LanguageId ?? DBNull.Value);
                var typeParam = new SqlParameter("@TypeName", (object?)options.TypeName ?? DBNull.Value);
                var pageParam = new SqlParameter("@PageNumber", options.PageNumber);
                var sizeParam = new SqlParameter("@PageSize", options.PageSize);

                var results = await _context.SearchIndexContents
                    .FromSqlRaw(@"
                        EXEC dbo.uspSearchIndex_Final 
                            @Keyword, 
                            @LanguageId, 
                            @TypeName, 
                            @PageNumber, 
                            @PageSize",
                        keywordParam,
                        languageParam,
                        typeParam,
                        pageParam,
                        sizeParam)
                    .ToListAsync(cancellationToken);

                // Get total count (execute SP again for count - stored procedure returns 2 result sets)
                var totalCount = await GetTotalCountAsync(keyword, options, cancellationToken);

                var result = new SearchResult
                {
                    Items = results,
                    TotalCount = totalCount,
                    PageNumber = options.PageNumber,
                    PageSize = options.PageSize
                };

                // Cache for 2 minutes - DISABLED FOR DEBUGGING
                // await _cache.SetStringAsync(
                //     cacheKey,
                //     JsonSerializer.Serialize(result),
                //     new DistributedCacheEntryOptions
                //     {
                //         AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(NORMAL_SEARCH_CACHE_SECONDS)
                //     },
                //     cancellationToken);

                // Log search query
                var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                await LogSearchQueryAsync(keyword, "normal", result.TotalCount, options, executionTime, cancellationToken);

                _logger.LogInformation("Normal search completed: {Keyword}, Results: {Count}, Time: {Ms}ms",
                    keyword, result.TotalCount, executionTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchNormalAsync for keyword: {Keyword}", keyword);
                throw;
            }
        }

        public async Task<SearchResult> SearchAIAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    _logger.LogWarning("Empty keyword provided to SearchAIAsync");
                    return new SearchResult();
                }

                // Check cache first
                var cacheKey = BuildCacheKey("ai", keyword, options);
                var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for AI search: {Keyword}", keyword);
                    return JsonSerializer.Deserialize<SearchResult>(cached)!;
                }

                _logger.LogInformation("Executing AI search for: {Keyword}", keyword);

                // Step 1: Get TOP 100 candidates from FullText
                var candidateOptions = new SearchOptions
                {
                    LanguageId = options.LanguageId,
                    TypeName = options.TypeName,
                    PageNumber = 1,
                    PageSize = 100
                };

                var keywordParam = new SqlParameter("@Keyword", keyword);
                var languageParam = new SqlParameter("@LanguageId", (object?)candidateOptions.LanguageId ?? DBNull.Value);
                var typeParam = new SqlParameter("@TypeName", (object?)candidateOptions.TypeName ?? DBNull.Value);
                var pageParam = new SqlParameter("@PageNumber", 1);
                var sizeParam = new SqlParameter("@PageSize", 100);

                var candidates = await _context.SearchIndexContents
                    .FromSqlRaw(@"
                        EXEC dbo.uspSearchIndex_Final 
                            @Keyword, 
                            @LanguageId, 
                            @TypeName, 
                            @PageNumber, 
                            @PageSize",
                        keywordParam,
                        languageParam,
                        typeParam,
                        pageParam,
                        sizeParam)
                    .ToListAsync(cancellationToken);

                _logger.LogDebug("AI search: Retrieved {Count} candidates from FullText", candidates.Count);

                // Step 2: TODO - Get query embedding and rerank by semantic similarity
                // For now, just use FullText ranking
                // In future: integrate with AISupplierMatchingService for semantic scoring

                // Step 3: Apply pagination to candidates
                var reranked = candidates
                    .Skip((options.PageNumber - 1) * options.PageSize)
                    .Take(options.PageSize)
                    .ToList();

                var result = new SearchResult
                {
                    Items = reranked,
                    TotalCount = candidates.Count,
                    PageNumber = options.PageNumber,
                    PageSize = options.PageSize
                };

                // Cache for 5 minutes
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(result),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(AI_SEARCH_CACHE_SECONDS)
                    },
                    cancellationToken);

                // Log search query
                var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                await LogSearchQueryAsync(keyword, "ai", result.TotalCount, options, executionTime, cancellationToken);

                _logger.LogInformation("AI search completed: {Keyword}, Results: {Count}, Time: {Ms}ms",
                    keyword, result.TotalCount, executionTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchAIAsync for keyword: {Keyword}", keyword);
                throw;
            }
        }

        /// <summary>
        /// Search using AI semantic search and group results by company/organization
        /// </summary>
        public async Task<List<ViewModel.AISearchResultGroup>> SearchAIGroupedAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Executing grouped AI search for: {Keyword}", keyword);

                // Step 1: Get AI search results
                var searchResult = await SearchAIAsync(keyword, options, cancellationToken);

                if (searchResult.Items == null || !searchResult.Items.Any())
                {
                    _logger.LogInformation("No AI search results to group");
                    return new List<ViewModel.AISearchResultGroup>();
                }

                // Step 2: Get product IDs from search results (RefId = SanPhamCNTB.ID)
                var productIds = searchResult.Items
                    .Where(item => item.RefId.HasValue && item.TypeName == "Công nghệ và Thiết bị")
                    .Select(item => (int)item.RefId!.Value)
                    .ToList();

                if (!productIds.Any())
                {
                    _logger.LogInformation("No product results to group");
                    return new List<ViewModel.AISearchResultGroup>();
                }

                // Step 3: Query SanPhamCNTB to get NCUId for each product
                var products = await _context.Set<Entities.SanPhamCNTB>()
                    .Where(p => productIds.Contains(p.ID))
                    .Select(p => new
                    {
                        p.ID,
                        p.NCUId,
                        p.Name,
                        p.URL,
                        p.Rating,
                        p.Viewed
                    })
                    .ToListAsync(cancellationToken);

                // Step 4: Get unique NCUIds
                var ncuIds = products
                    .Where(p => p.NCUId.HasValue)
                    .Select(p => p.NCUId!.Value)
                    .Distinct()
                    .ToList();

                if (!ncuIds.Any())
                {
                    _logger.LogInformation("No products have NCUId");
                    return new List<ViewModel.AISearchResultGroup>();
                }

                // Step 5: Query NhaCungUng to get company info
                var companies = await _context.Set<Entities.NhaCungUng>()
                    .Where(c => ncuIds.Contains(c.CungUngId))
                    .Select(c => new
                    {
                        c.CungUngId,
                        c.FullName,
                        c.QueryString,
                        c.Rating,
                        c.Viewed
                    })
                    .ToListAsync(cancellationToken);

                // Step 6: Create lookup dictionary for search results
                var searchResultLookup = searchResult.Items
                    .Where(item => item.RefId.HasValue)
                    .ToDictionary(item => (int)item.RefId!.Value, item => item);

                // Step 7: Group products by NCUId and create result groups
                var grouped = products
                    .Where(p => p.NCUId.HasValue)
                    .GroupBy(p => p.NCUId!.Value)
                    .Select(group =>
                    {
                        var company = companies.FirstOrDefault(c => c.CungUngId == group.Key);
                        if (company == null)
                            return null;

                        var productList = group.Select(product =>
                        {
                            // Get search result for relevance calculation
                            searchResultLookup.TryGetValue(product.ID, out var searchItem);

                            return new ViewModel.AIMatchedProduct
                            {
                                ProductId = product.ID,
                                ProductName = product.Name ?? string.Empty,
                                ProductUrl = product.URL ?? $"/san-pham/{product.ID}",
                                RelevancePercentage = CalculateRelevancePercentage(searchItem, keyword)
                            };
                        }).ToList();

                        return new ViewModel.AISearchResultGroup
                        {
                            CompanyId = company.CungUngId,
                            CompanyName = company.FullName ?? "Không rõ",
                            CompanyUrl = $"/nha-cung-ung/{company.QueryString ?? company.CungUngId.ToString()}",
                            MatchPercentage = CalculateMatchPercentage(productList),
                            Rating = company.Rating ?? 0,
                            ViewCount = company.Viewed ?? 0,
                            Products = productList.OrderByDescending(p => p.RelevancePercentage).ToList()
                        };
                    })
                    .Where(g => g != null)
                    .OrderByDescending(g => g!.MatchPercentage)
                    .ToList()!;

                _logger.LogInformation("Grouped AI search: {Companies} companies, {Products} total products",
                    grouped.Count, grouped.Sum(g => g.Products.Count));

                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchAIGroupedAsync for keyword: {Keyword}", keyword);
                throw;
            }
        }

        /// <summary>
        /// Calculate relevance percentage for a product (0-100)
        /// </summary>
        private double CalculateRelevancePercentage(SearchIndexContent? item, string keyword)
        {
            if (item == null)
                return 50.0;

            // Base score from FullText rank (if available)
            double score = 50.0; // Default mid-range

            // Boost if keyword appears in title
            if (!string.IsNullOrWhiteSpace(item.Title) && 
                item.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 30.0;
            }

            // Boost if keyword appears in description
            if (!string.IsNullOrWhiteSpace(item.Description) && 
                item.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 10.0;
            }

            // Normalize to 0-100 range
            return Math.Min(100.0, Math.Max(0.0, score));
        }

        /// <summary>
        /// Calculate overall match percentage for a company based on its products
        /// </summary>
        private double CalculateMatchPercentage(List<ViewModel.AIMatchedProduct> products)
        {
            if (products == null || !products.Any())
                return 0.0;

            // Average of top 3 product relevance scores
            var topScores = products
                .OrderByDescending(p => p.RelevancePercentage)
                .Take(3)
                .Select(p => p.RelevancePercentage)
                .ToList();

            return topScores.Average();
        }

        public async Task<List<SearchSuggestion>> GetSuggestionsAsync(
            string prefix,
            string? languageId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
                {
                    _logger.LogDebug("Prefix too short for suggestions: {Prefix}", prefix);
                    return new List<SearchSuggestion>();
                }

                // Check cache first
                var cacheKey = $"search:suggest:{prefix.ToLower()}:{languageId ?? "all"}";
                var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for suggestions: {Prefix}", prefix);
                    return JsonSerializer.Deserialize<List<SearchSuggestion>>(cached)!;
                }

                _logger.LogDebug("Executing autocomplete for: {Prefix}", prefix);

                // Execute stored procedure
                var prefixParam = new SqlParameter("@Prefix", prefix);
                var languageParam = new SqlParameter("@LanguageId", (object?)languageId ?? DBNull.Value);
                var topNParam = new SqlParameter("@TopN", 10);

                var suggestions = await _context.Database
                    .SqlQueryRaw<SearchSuggestionRaw>(@"
                        EXEC dbo.uspSearchSuggest 
                            @Prefix, 
                            @LanguageId, 
                            @TopN",
                        prefixParam,
                        languageParam,
                        topNParam)
                    .ToListAsync(cancellationToken);

                var result = suggestions.Select(s => new SearchSuggestion
                {
                    Id = s.Id,
                    Title = s.Title,
                    TypeName = s.TypeName,
                    URL = s.URL,
                    ImgPreview = s.ImgPreview,
                    Rank = s.RANK
                }).ToList();

                // Cache for 10 minutes
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(result),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(SUGGEST_CACHE_SECONDS)
                    },
                    cancellationToken);

                _logger.LogDebug("Autocomplete completed: {Prefix}, Suggestions: {Count}", prefix, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSuggestionsAsync for prefix: {Prefix}", prefix);
                return new List<SearchSuggestion>();
            }
        }

        public async Task<List<TrendingSearch>> GetTrendingSearchesAsync(
            int days = 7,
            int topN = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check cache first
                var cacheKey = $"search:trending:{days}:{topN}";
                var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for trending searches");
                    return JsonSerializer.Deserialize<List<TrendingSearch>>(cached)!;
                }

                _logger.LogDebug("Executing trending searches query");

                // Execute stored procedure
                var daysParam = new SqlParameter("@Days", days);
                var topNParam = new SqlParameter("@TopN", topN);

                var trending = await _context.Database
                    .SqlQueryRaw<TrendingSearch>(@"
                        EXEC dbo.uspSearchTrending 
                            @Days, 
                            @TopN",
                        daysParam,
                        topNParam)
                    .ToListAsync(cancellationToken);

                // Cache for 5 minutes
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(trending),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(TRENDING_CACHE_SECONDS)
                    },
                    cancellationToken);

                _logger.LogDebug("Trending searches completed: {Count} results", trending.Count);

                return trending;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTrendingSearchesAsync");
                return new List<TrendingSearch>();
            }
        }

        // =============================================
        // Private Helper Methods
        // =============================================

        private async Task<int> GetTotalCountAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken)
        {
            try
            {
                var keywordParam = new SqlParameter("@Keyword", keyword);
                var languageParam = new SqlParameter("@LanguageId", (object?)options.LanguageId ?? DBNull.Value);
                var typeParam = new SqlParameter("@TypeName", (object?)options.TypeName ?? DBNull.Value);

                // Execute count query
                var countResult = await _context.Database
                    .SqlQueryRaw<int>(@"
                        DECLARE @SearchTerm NVARCHAR(1000);
                        DECLARE @XML XML;
                        
                        -- Convert space-separated words to XML for splitting (SQL Server 2012 compatible)
                        SET @XML = CAST('<r><w>' + REPLACE(@Keyword, ' ', '</w><w>') + '</w></r>' AS XML);
                        
                        -- Build AND search term
                        SELECT @SearchTerm = STUFF((
                            SELECT ' AND ""' + LTRIM(RTRIM(T.c.value('.', 'NVARCHAR(500)'))) + '""'
                            FROM @XML.nodes('/r/w') T(c)
                            WHERE LTRIM(RTRIM(T.c.value('.', 'NVARCHAR(500)'))) <> ''
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'), 1, 5, ''); -- Remove leading ' AND '
                        
                        IF @SearchTerm IS NULL OR @SearchTerm = ''
                            SET @SearchTerm = '""' + @Keyword + '""';

                        SELECT COUNT(*) AS Value
                        FROM dbo.SearchIndexContents s
                        INNER JOIN CONTAINSTABLE(dbo.SearchIndexContents, (Title, RemovedUnicode, Contents), @SearchTerm) AS KEY_TBL
                            ON s.Id = KEY_TBL.[KEY]
                        WHERE 
                            (@LanguageId IS NULL OR s.LanguageId = @LanguageId)
                            AND (@TypeName IS NULL OR s.TypeName = @TypeName);",
                        keywordParam,
                        languageParam,
                        typeParam)
                    .FirstOrDefaultAsync(cancellationToken);

                return countResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total count for keyword: {Keyword}", keyword);
                return 0;
            }
        }

        private async Task LogSearchQueryAsync(
            string keyword,
            string mode,
            int resultCount,
            SearchOptions options,
            int executionTimeMs,
            CancellationToken cancellationToken)
        {
            try
            {
                var normalizedKeyword = NormalizeKeyword(keyword);

                var keywordParam = new SqlParameter("@Keyword", keyword);
                var normalizedParam = new SqlParameter("@NormalizedKeyword", normalizedKeyword);
                var resultCountParam = new SqlParameter("@ResultCount", resultCount);
                var modeParam = new SqlParameter("@SearchMode", mode);
                var languageParam = new SqlParameter("@LanguageId", (object?)options.LanguageId ?? DBNull.Value);
                var typeParam = new SqlParameter("@TypeName", (object?)options.TypeName ?? DBNull.Value);
                var userAgentParam = new SqlParameter("@UserAgent", DBNull.Value);
                var ipParam = new SqlParameter("@IpAddress", DBNull.Value);
                var timeParam = new SqlParameter("@ExecutionTimeMs", executionTimeMs);

                await _context.Database.ExecuteSqlRawAsync(@"
                    EXEC dbo.uspLogSearchQuery 
                        @Keyword, 
                        @NormalizedKeyword, 
                        @ResultCount, 
                        @SearchMode, 
                        @LanguageId, 
                        @TypeName, 
                        @UserAgent, 
                        @IpAddress, 
                        @ExecutionTimeMs",
                    keywordParam,
                    normalizedParam,
                    resultCountParam,
                    modeParam,
                    languageParam,
                    typeParam,
                    userAgentParam,
                    ipParam,
                    timeParam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging search query: {Keyword}", keyword);
                // Don't throw - logging failure shouldn't break search
            }
        }

        private string BuildCacheKey(string mode, string keyword, SearchOptions options)
        {
            var hash = ComputeHash(keyword);
            return $"search:{mode}:{options.LanguageId ?? "all"}:{options.TypeName ?? "all"}:{options.PageNumber}:{options.PageSize}:{hash}";
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input.ToLower());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash)[..16]; // First 16 chars
        }

        private string NormalizeKeyword(string keyword)
        {
            // Remove accents and convert to lowercase
            return RemoveVietnameseAccents(keyword).ToLower().Trim();
        }

        private string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = char.GetUnicodeCategory(c);
                if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // Helper class for raw SQL query results
        private class SearchSuggestionRaw
        {
            public long Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? TypeName { get; set; }
            public string? URL { get; set; }
            public string? ImgPreview { get; set; }
            public int RANK { get; set; }
        }
    }
}
