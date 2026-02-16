namespace TPApp.ViewModel
{
    /// <summary>
    /// Represents a group of AI search results for a single company/organization
    /// </summary>
    public class AISearchResultGroup
    {
        /// <summary>
        /// Company/Organization ID
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// Company/Organization name
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// URL to company/organization page
        /// </summary>
        public string CompanyUrl { get; set; } = string.Empty;

        /// <summary>
        /// Company rating (0-5 stars)
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Number of views
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// Overall match percentage for this company (0-100)
        /// </summary>
        public double MatchPercentage { get; set; }

        /// <summary>
        /// List of matched products for this company
        /// </summary>
        public List<AIMatchedProduct> Products { get; set; } = new();
    }

    /// <summary>
    /// Represents a single product matched in AI search
    /// </summary>
    public class AIMatchedProduct
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// URL to product page
        /// </summary>
        public string ProductUrl { get; set; } = string.Empty;

        /// <summary>
        /// Relevance percentage for this product (0-100)
        /// </summary>
        public double RelevancePercentage { get; set; }
    }
}
