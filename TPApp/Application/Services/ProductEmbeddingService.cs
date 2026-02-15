using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TPApp.Data;
using TPApp.Domain.Entities;
using TPApp.Infrastructure.AI;
using TPApp.Infrastructure.Repositories;

namespace TPApp.Application.Services
{
    /// <summary>
    /// Service for generating product embeddings by combining product and category data.
    /// Includes safe text building, truncation, and token limit protection.
    /// </summary>
    public class ProductEmbeddingService : IProductEmbeddingService
    {
        private readonly AppDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly ILogger<ProductEmbeddingService> _logger;

        private const int MAX_TEXT_LENGTH = 6000; // Safe limit well below 8192 tokens
        private const int MIN_TEXT_LENGTH = 20; // Minimum meaningful text
        private const int THONGSO_LIMIT = 2000; // Limit for specifications
        private const int UUDIEM_LIMIT = 1500; // Limit for advantages

        public ProductEmbeddingService(
            AppDbContext context,
            IEmbeddingService embeddingService,
            IEmbeddingRepository embeddingRepository,
            ILogger<ProductEmbeddingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _embeddingRepository = embeddingRepository ?? throw new ArgumentNullException(nameof(embeddingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateProductEmbeddingTextAsync(
            int productId, 
            CancellationToken cancellationToken = default)
        {
            var product = await _context.SanPhamCNTBs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == productId, cancellationToken);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} not found");
            }

            // Load categories
            Entities.Category? primaryCategory = null;
            if (!string.IsNullOrWhiteSpace(product.CategoryId))
            {
                var categoryIds = product.CategoryId
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim())
                    .Where(id => int.TryParse(id, out _))
                    .Select(int.Parse)
                    .ToList();

                if (categoryIds.Any())
                {
                    primaryCategory = await _context.Categories
                        .Where(c => categoryIds.Contains(c.CatId))
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            // Build embedding text with safe limits
            var embeddingText = BuildEmbeddingText(product, primaryCategory);
            var originalLength = embeddingText.Length;

            // Apply safe truncation
            embeddingText = TruncateSafely(embeddingText, MAX_TEXT_LENGTH);

            if (embeddingText.Length < originalLength)
            {
                _logger.LogWarning(
                    "Product {ProductId} text truncated from {OriginalLength} to {TruncatedLength} characters",
                    productId, originalLength, embeddingText.Length);
            }

            _logger.LogDebug(
                "Product {ProductId} embedding text prepared: {Length} characters",
                productId, embeddingText.Length);

            return embeddingText;
        }

        public async Task UpdateProductEmbeddingAsync(int productId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Updating embedding for product {ProductId}", productId);

            var product = await _context.SanPhamCNTBs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == productId, cancellationToken);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} not found");
            }

            if (!product.NCUId.HasValue)
            {
                _logger.LogWarning("Product {ProductId} has no supplier (NCUId is null), skipping", productId);
                return;
            }

            var embeddingText = await GenerateProductEmbeddingTextAsync(productId, cancellationToken);
            var textLength = embeddingText.Length;

            // Prevent empty embedding calls
            if (textLength < MIN_TEXT_LENGTH)
            {
                _logger.LogWarning(
                    "Product {ProductId} text too short ({Length} chars), skipping embedding generation",
                    productId, textLength);
                return;
            }

            _logger.LogInformation(
                "Generating embedding for product {ProductId} with {Length} characters",
                productId, textLength);

            var embeddingVector = await _embeddingService.GenerateEmbeddingAsync(embeddingText, cancellationToken);

            var embeddingJson = JsonSerializer.Serialize(embeddingVector);

            var embedding = new SanPhamEmbedding
            {
                SanPhamId = productId,
                NCUId = product.NCUId.Value,
                Embedding = embeddingJson,
                UpdatedDate = DateTime.UtcNow
            };

            await _embeddingRepository.UpsertEmbeddingAsync(embedding, cancellationToken);

            _logger.LogInformation(
                "Successfully updated embedding for product {ProductId} ({Dimensions} dimensions)",
                productId, embeddingVector.Length);
        }

        /// <summary>
        /// Builds embedding text from product and category data in priority order.
        /// Applies field limits to prevent token overflow.
        /// </summary>
        private string BuildEmbeddingText(Entities.SanPhamCNTB product, Entities.Category? category)
        {
            var sb = new StringBuilder();

            // Priority 1: Name (mandatory)
            if (!string.IsNullOrWhiteSpace(product.Name))
            {
                sb.AppendLine(CleanText(product.Name));
            }

            // Priority 2: Keywords
            if (!string.IsNullOrWhiteSpace(product.Keywords))
            {
                sb.AppendLine(CleanText(product.Keywords));
            }

            // Priority 3: Description
            if (!string.IsNullOrWhiteSpace(product.MoTa))
            {
                sb.AppendLine(CleanText(product.MoTa));
            }

            // Priority 4: Category Title
            if (category != null && !string.IsNullOrWhiteSpace(category.Title))
            {
                sb.AppendLine(CleanText(category.Title));
            }

            // Priority 5: Category Description
            if (category != null && !string.IsNullOrWhiteSpace(category.Description))
            {
                sb.AppendLine(CleanText(category.Description));
            }

            // Priority 6: Specifications (limited)
            if (!string.IsNullOrWhiteSpace(product.ThongSo))
            {
                var limitedThongSo = LimitField(product.ThongSo, THONGSO_LIMIT);
                sb.AppendLine(CleanText(limitedThongSo));
            }

            // Priority 7: Advantages (limited)
            if (!string.IsNullOrWhiteSpace(product.UuDiem))
            {
                var limitedUuDiem = LimitField(product.UuDiem, UUDIEM_LIMIT);
                sb.AppendLine(CleanText(limitedUuDiem));
            }

            var result = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(result))
            {
                return "No description available";
            }

            return result;
        }

        /// <summary>
        /// Truncates text safely without breaking UTF-8 encoding.
        /// Removes duplicate spaces and trims whitespace.
        /// </summary>
        private string TruncateSafely(string input, int maxLength = MAX_TEXT_LENGTH)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Trim and remove duplicate spaces
            var cleaned = Regex.Replace(input.Trim(), @"\s+", " ");

            if (cleaned.Length <= maxLength)
            {
                return cleaned;
            }

            // Truncate safely
            var truncated = cleaned.Substring(0, maxLength);

            // Try to break at last space to avoid cutting words
            var lastSpace = truncated.LastIndexOf(' ');
            if (lastSpace > maxLength - 100) // Only if space is near the end
            {
                truncated = truncated.Substring(0, lastSpace);
            }

            return truncated.Trim();
        }

        /// <summary>
        /// Limits a field to a maximum length.
        /// </summary>
        private string LimitField(string input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var trimmed = input.Trim();
            if (trimmed.Length <= maxLength)
            {
                return trimmed;
            }

            return trimmed.Substring(0, maxLength).Trim();
        }

        /// <summary>
        /// Cleans text by removing excessive whitespace and normalizing.
        /// </summary>
        private string CleanText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Remove HTML tags if present
            var cleaned = Regex.Replace(input, @"<[^>]+>", " ");

            // Remove duplicate spaces, tabs, newlines
            cleaned = Regex.Replace(cleaned, @"\s+", " ");

            return cleaned.Trim();
        }
    }
}
