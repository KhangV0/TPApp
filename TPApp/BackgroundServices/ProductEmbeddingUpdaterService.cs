using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TPApp.Application.Services;
using TPApp.Configuration;
using TPApp.Data;

namespace TPApp.BackgroundServices
{
    /// <summary>
    /// Background service that generates embeddings for products on application startup.
    /// Processes products in batches to avoid rate limiting.
    /// LimitSearchProduct: -1 = embed all, positive number = limit to N newest products
    /// </summary>
    public class ProductEmbeddingUpdaterService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductEmbeddingUpdaterService> _logger;
        private readonly FeatureFlags _featureFlags;
        private const int BATCH_SIZE = 10;
        private const int DELAY_BETWEEN_BATCHES_MS = 2000;

        public ProductEmbeddingUpdaterService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ProductEmbeddingUpdaterService> logger,
            IOptions<FeatureFlags> featureFlags)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _featureFlags = featureFlags?.Value ?? new FeatureFlags();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Early exit if background job is disabled via feature flag
            if (_featureFlags.EnableEmbeddingBackgroundJob == 0)
            {
                _logger.LogInformation("Embedding background job disabled via config (EnableEmbeddingBackgroundJob = 0). Skipping startup.");
                return;
            }

            _logger.LogInformation("ProductEmbeddingUpdaterService starting...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var embeddingService = scope.ServiceProvider.GetRequiredService<IProductEmbeddingService>();

                // Read LimitSearchProduct from configuration
                var limitSearchProduct = _configuration.GetValue<int>("OpenAI:LimitSearchProduct", 1000);
                
                _logger.LogInformation(
                    "Querying for newest products with StatusId = 3 without embeddings (Limit: {Limit})...",
                    limitSearchProduct == -1 ? "ALL" : limitSearchProduct.ToString());

                // Build query for products without embeddings
                var query = from s in context.SanPhamCNTBs
                            where s.StatusId == 3
                            join e in context.SanPhamEmbeddings
                                on s.ID equals e.SanPhamId into gj
                            from sub in gj.DefaultIfEmpty()
                            where sub == null
                            orderby s.Created descending
                            select s.ID;

                // Apply limit if not -1 (unlimited)
                if (limitSearchProduct > 0)
                {
                    query = query.Take(limitSearchProduct);
                }

                var productsWithoutEmbeddings = await query.ToListAsync(cancellationToken);

                // Safety guard: no products found
                if (productsWithoutEmbeddings.Count == 0)
                {
                    _logger.LogInformation("No products found that need embeddings. All products are up to date.");
                    return;
                }

                var limitInfo = limitSearchProduct == -1 ? "unlimited" : $"limited to {limitSearchProduct} newest";
                _logger.LogInformation("Found {Count} products without embeddings ({LimitInfo}). Starting batch processing...", 
                    productsWithoutEmbeddings.Count, limitInfo);

                var totalBatches = (int)Math.Ceiling(productsWithoutEmbeddings.Count / (double)BATCH_SIZE);
                var successCount = 0;
                var failedCount = 0;

                for (int i = 0; i < productsWithoutEmbeddings.Count; i += BATCH_SIZE)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Embedding generation cancelled after processing {Success} successful, {Failed} failed", 
                            successCount, failedCount);
                        break;
                    }

                    var batch = productsWithoutEmbeddings.Skip(i).Take(BATCH_SIZE).ToList();
                    var currentBatch = (i / BATCH_SIZE) + 1;

                    _logger.LogInformation("Processing batch {Current}/{Total} ({Count} products)", 
                        currentBatch, totalBatches, batch.Count);

                    foreach (var productId in batch)
                    {
                        try
                        {
                            _logger.LogDebug("Generating embedding for product {ProductId}", productId);
                            await embeddingService.UpdateProductEmbeddingAsync(productId, cancellationToken);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to generate embedding for product {ProductId}", productId);
                            failedCount++;
                        }
                    }

                    if (i + BATCH_SIZE < productsWithoutEmbeddings.Count)
                    {
                        _logger.LogDebug("Waiting {Delay}ms before next batch to avoid rate limiting...", 
                            DELAY_BETWEEN_BATCHES_MS);
                        await Task.Delay(DELAY_BETWEEN_BATCHES_MS, cancellationToken);
                    }
                }

                _logger.LogInformation(
                    "Embedding generation completed. Total selected: {Total}, Success: {Success}, Failed: {Failed}", 
                    productsWithoutEmbeddings.Count, successCount, failedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProductEmbeddingUpdaterService");
            }

            return;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductEmbeddingUpdaterService stopping...");
            return Task.CompletedTask;
        }
    }
}
