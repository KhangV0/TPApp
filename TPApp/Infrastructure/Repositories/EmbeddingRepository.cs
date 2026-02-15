using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Domain.Entities;

namespace TPApp.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of the embedding repository.
    /// </summary>
    public class EmbeddingRepository : IEmbeddingRepository
    {
        private readonly AppDbContext _context;

        public EmbeddingRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<SanPhamEmbedding>> GetAllEmbeddingsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SanPhamEmbeddings
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertEmbeddingAsync(SanPhamEmbedding embedding, CancellationToken cancellationToken = default)
        {
            if (embedding == null)
            {
                throw new ArgumentNullException(nameof(embedding));
            }

            var existing = await _context.SanPhamEmbeddings
                .FindAsync(new object[] { embedding.SanPhamId }, cancellationToken);

            if (existing != null)
            {
                existing.Embedding = embedding.Embedding;
                existing.NCUId = embedding.NCUId;
                existing.UpdatedDate = DateTime.UtcNow;
                _context.SanPhamEmbeddings.Update(existing);
            }
            else
            {
                embedding.UpdatedDate = DateTime.UtcNow;
                await _context.SanPhamEmbeddings.AddAsync(embedding, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<SanPhamEmbedding?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.SanPhamEmbeddings
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.SanPhamId == productId, cancellationToken);
        }
    }
}
