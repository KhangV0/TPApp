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
    /// Background service that generates embeddings for products AFTER app is fully started.
    /// Inherits BackgroundService so ExecuteAsync runs in background — does NOT block startup.
    /// LimitSearchProduct: -1 = embed all, positive number = limit to N newest products.
    /// </summary>
    public class ProductEmbeddingUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductEmbeddingUpdaterService> _logger;
        private readonly FeatureFlags _featureFlags;

        private const int BATCH_SIZE = 10;
        private const int DELAY_BETWEEN_BATCHES_MS = 2000;
        private const int STARTUP_DELAY_SECONDS = 15; // Wait for app to fully start before heavy work

        public ProductEmbeddingUpdaterService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ProductEmbeddingUpdaterService> logger,
            IOptions<FeatureFlags> featureFlags)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration   = configuration   ?? throw new ArgumentNullException(nameof(configuration));
            _logger          = logger           ?? throw new ArgumentNullException(nameof(logger));
            _featureFlags    = featureFlags?.Value ?? new FeatureFlags();
        }

        /// <summary>
        /// Runs entirely in background — does NOT block ASP.NET Core startup.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Early exit if disabled
            if (_featureFlags.EnableEmbeddingBackgroundJob == 0)
            {
                _logger.LogInformation("Embedding background job disabled (EnableEmbeddingBackgroundJob = 0). Skipping.");
                return;
            }

            // Wait for app to finish starting up before doing heavy OpenAI calls
            _logger.LogInformation(
                "ProductEmbeddingUpdaterService: waiting {Delay}s for app startup before starting embeddings...",
                STARTUP_DELAY_SECONDS);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(STARTUP_DELAY_SECONDS), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return; // App shutting down before we even start — that's fine
            }

            _logger.LogInformation("ProductEmbeddingUpdaterService: starting embedding generation...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var embeddingService = scope.ServiceProvider.GetRequiredService<IProductEmbeddingService>();

                var limitSearchProduct = _configuration.GetValue<int>("OpenAI:LimitSearchProduct", 1000);

                _logger.LogInformation(
                    "Querying products without embeddings (StatusId=3, limit: {Limit})...",
                    limitSearchProduct == -1 ? "ALL" : limitSearchProduct.ToString());

                var query = from s in context.SanPhamCNTBs
                            where s.StatusId == 3
                            join e in context.SanPhamEmbeddings
                                on s.ID equals e.SanPhamId into gj
                            from sub in gj.DefaultIfEmpty()
                            where sub == null
                            orderby s.Created descending
                            select s.ID;

                if (limitSearchProduct > 0)
                    query = query.Take(limitSearchProduct);

                var productsWithoutEmbeddings = await query.ToListAsync(stoppingToken);

                if (productsWithoutEmbeddings.Count == 0)
                {
                    _logger.LogInformation("No products need embeddings. Done.");
                    return;
                }

                _logger.LogInformation(
                    "Found {Count} products without embeddings. Processing in batches of {BatchSize}...",
                    productsWithoutEmbeddings.Count, BATCH_SIZE);

                var successCount = 0;
                var failedCount  = 0;
                var totalBatches = (int)Math.Ceiling(productsWithoutEmbeddings.Count / (double)BATCH_SIZE);

                for (int i = 0; i < productsWithoutEmbeddings.Count; i += BATCH_SIZE)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Embedding cancelled — success: {S}, failed: {F}", successCount, failedCount);
                        break;
                    }

                    var batch        = productsWithoutEmbeddings.Skip(i).Take(BATCH_SIZE).ToList();
                    var currentBatch = (i / BATCH_SIZE) + 1;

                    _logger.LogInformation("Batch {Current}/{Total} ({Count} products)", currentBatch, totalBatches, batch.Count);

                    foreach (var productId in batch)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        try
                        {
                            await embeddingService.UpdateProductEmbeddingAsync(productId, stoppingToken);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to embed product {ProductId}", productId);
                            failedCount++;
                        }
                    }

                    if (i + BATCH_SIZE < productsWithoutEmbeddings.Count)
                        await Task.Delay(DELAY_BETWEEN_BATCHES_MS, stoppingToken);
                }

                _logger.LogInformation(
                    "Embedding done. Total: {Total}, Success: {S}, Failed: {F}",
                    productsWithoutEmbeddings.Count, successCount, failedCount);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ProductEmbeddingUpdaterService stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in ProductEmbeddingUpdaterService");
            }
        }
    }
}
