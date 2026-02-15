using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TPApp.Domain.Models;

namespace TPApp.Application.Services
{
    /// <summary>
    /// Service for matching suppliers to buyer queries using AI semantic search.
    /// </summary>
    public interface IAISupplierMatchingService
    {
        /// <summary>
        /// Finds the top matching suppliers for a given query text.
        /// </summary>
        /// <param name="queryText">The buyer's technology request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of top 10 suppliers ranked by hybrid score</returns>
        Task<List<SupplierMatchResult>> FindMatchingSuppliersAsync(
            string queryText, 
            CancellationToken cancellationToken = default);
    }
}
