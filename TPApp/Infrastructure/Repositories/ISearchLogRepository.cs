using System.Threading;
using System.Threading.Tasks;

namespace TPApp.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for logging AI search queries.
    /// </summary>
    public interface ISearchLogRepository
    {
        /// <summary>
        /// Logs a search query with its result count.
        /// </summary>
        Task LogSearchAsync(string queryText, int resultCount, CancellationToken cancellationToken = default);
    }
}
