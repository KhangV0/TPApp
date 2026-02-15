namespace TPApp.Domain.Models
{
    /// <summary>
    /// Represents a product matched to a search query with its similarity score.
    /// </summary>
    public class ProductMatchResult
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cosine similarity score between query and product (0-1)
        /// </summary>
        public double SimilarityScore { get; set; }
    }
}
