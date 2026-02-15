using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TPApp.Configuration;
using TPApp.Data;
using TPApp.Domain.Entities;
using TPApp.Domain.Models;
using TPApp.Infrastructure.AI;
using TPApp.Infrastructure.Repositories;

namespace TPApp.Application.Services
{
    /// <summary>
    /// Implementation of AI supplier matching service with hybrid ranking algorithm.
    /// </summary>
    public class AISupplierMatchingService : IAISupplierMatchingService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly ISearchLogRepository _searchLogRepository;
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AISupplierMatchingService> _logger;
        private readonly FeatureFlags _featureFlags;

        private const string EMBEDDINGS_CACHE_KEY = "all_product_embeddings";
        private const int CACHE_DURATION_MINUTES = 10;
        private const int TOP_PRODUCTS_COUNT = 50;
        private const int TOP_SUPPLIERS_COUNT = 10;
        private const int TOP_PRODUCTS_PER_SUPPLIER = 3;

        public AISupplierMatchingService(
            IEmbeddingService embeddingService,
            IEmbeddingRepository embeddingRepository,
            ISearchLogRepository searchLogRepository,
            AppDbContext context,
            IMemoryCache cache,
            ILogger<AISupplierMatchingService> logger,
            IOptions<FeatureFlags> featureFlags)
        {
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _embeddingRepository = embeddingRepository ?? throw new ArgumentNullException(nameof(embeddingRepository));
            _searchLogRepository = searchLogRepository ?? throw new ArgumentNullException(nameof(searchLogRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _featureFlags = featureFlags?.Value ?? new FeatureFlags();
        }

        public async Task<List<SupplierMatchResult>> FindMatchingSuppliersAsync(
            string queryText, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                throw new ArgumentException("Query text cannot be null or empty", nameof(queryText));
            }

            _logger.LogInformation("Finding matching suppliers for query: {Query}", queryText);

            // Step 1: Generate embedding for buyer input
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(queryText, cancellationToken);

            // Step 2: Load all embeddings from cache
            var allEmbeddings = await GetAllEmbeddingsFromCacheAsync(cancellationToken);

            if (allEmbeddings.Count == 0)
            {
                _logger.LogWarning("No product embeddings found in database");
                await _searchLogRepository.LogSearchAsync(queryText, 0, cancellationToken);
                return new List<SupplierMatchResult>();
            }

            // Step 3: Calculate cosine similarity for each product
            var productScores = new List<(int ProductId, int NCUId, double Score)>();

            foreach (var embedding in allEmbeddings)
            {
                try
                {
                    var productEmbedding = JsonSerializer.Deserialize<float[]>(embedding.Embedding);
                    if (productEmbedding != null)
                    {
                        var similarity = CalculateCosineSimilarity(queryEmbedding, productEmbedding);
                        productScores.Add((embedding.SanPhamId, embedding.NCUId, similarity));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize embedding for product {ProductId}", 
                        embedding.SanPhamId);
                }
            }

            // Step 4: Take top 50 products by similarity
            var topProducts = productScores
                .OrderByDescending(p => p.Score)
                .Take(TOP_PRODUCTS_COUNT)
                .ToList();

            if (topProducts.Count == 0)
            {
                _logger.LogWarning("No products matched the query");
                await _searchLogRepository.LogSearchAsync(queryText, 0, cancellationToken);
                return new List<SupplierMatchResult>();
            }

            // Step 5: Group by supplier and calculate hybrid scores
            var supplierGroups = topProducts
                .GroupBy(p => p.NCUId)
                .Select(g => new
                {
                    NCUId = g.Key,
                    MaxScore = g.Max(p => p.Score),
                    AverageTop3Score = g.OrderByDescending(p => p.Score).Take(3).Average(p => p.Score),
                    TopProductIds = g.OrderByDescending(p => p.Score).Take(TOP_PRODUCTS_PER_SUPPLIER).ToList()
                })
                .ToList();

            // Step 6: Load supplier details and calculate final scores
            var supplierIds = supplierGroups.Select(g => g.NCUId).ToList();
            var suppliers = await _context.NhaCungUngs
                .Where(s => supplierIds.Contains(s.CungUngId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var maxViewed = suppliers.Max(s => s.Viewed ?? 0);

            var results = new List<SupplierMatchResult>();

            foreach (var group in supplierGroups)
            {
                var supplier = suppliers.FirstOrDefault(s => s.CungUngId == group.NCUId);
                if (supplier == null) continue;

                var rating = supplier.Rating ?? 0;
                var viewed = supplier.Viewed ?? 0;

                var finalScore = CalculateHybridScore(
                    group.MaxScore,
                    group.AverageTop3Score,
                    rating,
                    viewed,
                    maxViewed);

                // Load product details for top products
                var productIds = group.TopProductIds.Select(p => p.ProductId).ToList();
                var products = await _context.SanPhamCNTBs
                    .Where(p => productIds.Contains(p.ID))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var topProductResults = group.TopProductIds
                    .Select(p => new ProductMatchResult
                    {
                        ProductId = p.ProductId,
                        Name = products.FirstOrDefault(pr => pr.ID == p.ProductId)?.Name ?? "Unknown",
                        SimilarityScore = p.Score
                    })
                    .ToList();

                results.Add(new SupplierMatchResult
                {
                    CungUngId = supplier.CungUngId,
                    SupplierName = supplier.FullName ?? "Unknown",
                    FinalScore = finalScore,
                    SemanticScore = group.MaxScore,
                    Rating = rating,
                    Viewed = viewed,
                    TopProducts = topProductResults
                });
            }

            // Step 7: Sort by final score and take top 10
            var topSuppliers = results
                .OrderByDescending(r => r.FinalScore)
                .Take(TOP_SUPPLIERS_COUNT)
                .ToList();

            // Step 8: Log search
            await _searchLogRepository.LogSearchAsync(queryText, topSuppliers.Count, cancellationToken);

            _logger.LogInformation("Found {Count} matching suppliers", topSuppliers.Count);

            return topSuppliers;
        }

        /// <summary>
        /// Calculates cosine similarity between two vectors.
        /// Returns a value between 0 and 1, where 1 means identical vectors.
        /// </summary>
        public static double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1 == null || vector2 == null)
            {
                return 0.0;
            }

            if (vector1.Length != vector2.Length)
            {
                return 0.0;
            }

            double dotProduct = 0.0;
            double magnitude1 = 0.0;
            double magnitude2 = 0.0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0.0 || magnitude2 == 0.0)
            {
                return 0.0;
            }

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Calculates hybrid score combining semantic similarity with supplier metrics.
        /// Formula: 0.6 * MaxScore + 0.2 * AvgTop3 + 0.1 * (Rating/5) + 0.1 * (Viewed/MaxViewed)
        /// </summary>
        public static double CalculateHybridScore(
            double maxProductScore,
            double averageTop3Score,
            int rating,
            int viewed,
            int maxViewed)
        {
            var normalizedRating = rating / 5.0;
            var normalizedViewed = maxViewed > 0 ? (double)viewed / maxViewed : 0.0;

            return (0.6 * maxProductScore) +
                   (0.2 * averageTop3Score) +
                   (0.1 * normalizedRating) +
                   (0.1 * normalizedViewed);
        }

        private async Task<List<SanPhamEmbedding>> GetAllEmbeddingsFromCacheAsync(
            CancellationToken cancellationToken)
        {
            // Check if cache is enabled via feature flag
            if (_featureFlags.EnableCache == 1)
            {
                // Cache enabled - try to get from cache first
                if (_cache.TryGetValue<List<SanPhamEmbedding>>(EMBEDDINGS_CACHE_KEY, out var cached) && cached != null)
                {
                    _logger.LogDebug("Retrieved {Count} embeddings from cache", cached.Count);
                    return cached;
                }

                _logger.LogDebug("Cache miss, loading embeddings from database with StatusId = 3 filter");
            }
            else
            {
                _logger.LogDebug("Cache disabled by feature flag, loading embeddings from database");
            }
            
            // Load from database
            var embeddings = await _context.SanPhamEmbeddings
                .Join(
                    _context.SanPhamCNTBs,
                    embedding => embedding.SanPhamId,
                    product => product.ID,
                    (embedding, product) => new { Embedding = embedding, Product = product }
                )
                .Where(joined => joined.Product.StatusId == 3)
                .Select(joined => joined.Embedding)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Only cache if feature flag is enabled
            if (_featureFlags.EnableCache == 1)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                _cache.Set(EMBEDDINGS_CACHE_KEY, embeddings, cacheOptions);

                _logger.LogDebug("Cached {Count} embeddings (StatusId = 3 only) for {Minutes} minutes", 
                    embeddings.Count, CACHE_DURATION_MINUTES);
            }
            else
            {
                _logger.LogDebug("Loaded {Count} embeddings from database (cache disabled)", embeddings.Count);
            }

            return embeddings;
        }
    }
}
