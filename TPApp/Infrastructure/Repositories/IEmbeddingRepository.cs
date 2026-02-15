using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TPApp.Domain.Entities;

namespace TPApp.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for managing product embeddings.
    /// </summary>
    public interface IEmbeddingRepository
    {
        /// <summary>
        /// Gets all product embeddings from the database.
        /// </summary>
        Task<List<SanPhamEmbedding>> GetAllEmbeddingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts or updates a product embedding.
        /// </summary>
        Task UpsertEmbeddingAsync(SanPhamEmbedding embedding, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an embedding by product ID.
        /// </summary>
        Task<SanPhamEmbedding?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    }
}
