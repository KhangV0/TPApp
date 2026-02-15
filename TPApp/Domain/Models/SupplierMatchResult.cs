using System.Collections.Generic;

namespace TPApp.Domain.Models
{
    /// <summary>
    /// Represents a supplier matched to a search query with semantic similarity and hybrid ranking.
    /// </summary>
    public class SupplierMatchResult
    {
        /// <summary>
        /// Supplier ID
        /// </summary>
        public int CungUngId { get; set; }

        /// <summary>
        /// Supplier name
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;

        /// <summary>
        /// Final hybrid score (0-1) combining semantic similarity, rating, and views
        /// </summary>
        public double FinalScore { get; set; }

        /// <summary>
        /// Maximum semantic similarity score among all products from this supplier (0-1)
        /// </summary>
        public double SemanticScore { get; set; }

        /// <summary>
        /// Supplier rating (1-5)
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Number of views
        /// </summary>
        public int Viewed { get; set; }

        /// <summary>
        /// Top 3 products from this supplier that match the query
        /// </summary>
        public List<ProductMatchResult> TopProducts { get; set; } = new List<ProductMatchResult>();
    }
}
