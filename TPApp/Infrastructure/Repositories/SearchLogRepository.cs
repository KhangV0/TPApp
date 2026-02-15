using System;
using System.Threading;
using System.Threading.Tasks;
using TPApp.Data;
using TPApp.Domain.Entities;

namespace TPApp.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of the search log repository.
    /// </summary>
    public class SearchLogRepository : ISearchLogRepository
    {
        private readonly AppDbContext _context;

        public SearchLogRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task LogSearchAsync(string queryText, int resultCount, CancellationToken cancellationToken = default)
        {
            var log = new AISearchLog
            {
                QueryText = queryText ?? string.Empty,
                ResultCount = resultCount,
                CreatedDate = DateTime.UtcNow
            };

            await _context.AISearchLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
