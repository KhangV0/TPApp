namespace TPApp.Configuration
{
    /// <summary>
    /// Feature flags for controlling search functionality at runtime via appsettings.json
    /// </summary>
    public class FeatureFlags
    {
        /// <summary>
        /// Enable/disable AI semantic search (0 = disabled, 1 = enabled)
        /// Default: 0 (disabled for safety)
        /// </summary>
        public int EnableAISearch { get; set; } = 0;

        /// <summary>
        /// Enable/disable embedding background job (0 = disabled, 1 = enabled)
        /// Default: 0 (disabled for safety)
        /// </summary>
        public int EnableEmbeddingBackgroundJob { get; set; } = 0;

        /// <summary>
        /// Minimum query length required for AI search
        /// Default: 5 characters
        /// </summary>
        public int MinAISearchLength { get; set; } = 5;

        /// <summary>
        /// Enable/disable keyword search (0 = disabled, 1 = enabled)
        /// Default: 1 (enabled)
        /// </summary>
        public int EnableKeywordSearch { get; set; } = 1;

        /// <summary>
        /// Maximum number of AI search results to return
        /// Default: 10 suppliers
        /// </summary>
        public int MaxAISearchResults { get; set; } = 10;

        /// <summary>
        /// Enable/disable embedding cache (0 = disabled, 1 = enabled)
        /// Default: 1 (enabled for performance)
        /// </summary>
        public int EnableCache { get; set; } = 1;
    }
}
