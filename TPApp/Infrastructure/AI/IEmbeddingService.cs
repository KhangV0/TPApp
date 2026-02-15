using System.Threading;
using System.Threading.Tasks;

namespace TPApp.Infrastructure.AI
{
    /// <summary>
    /// Service for generating text embeddings using AI models.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generates an embedding vector for the given text.
        /// </summary>
        /// <param name="text">The text to generate an embedding for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A float array representing the embedding vector (1536 dimensions for text-embedding-3-small)</returns>
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}
