using System.Threading;
using System.Threading.Tasks;

namespace TPApp.Application.Services
{
    /// <summary>
    /// Service for generating and updating product embeddings.
    /// </summary>
    public interface IProductEmbeddingService
    {
        /// <summary>
        /// Generates the combined text for embedding from product data.
        /// </summary>
        Task<string> GenerateProductEmbeddingTextAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the embedding for a specific product.
        /// </summary>
        Task UpdateProductEmbeddingAsync(int productId, CancellationToken cancellationToken = default);
    }
}
