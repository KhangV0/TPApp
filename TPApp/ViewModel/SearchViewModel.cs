using System.Collections.Generic;
using TPApp.Domain.Models;

namespace TPApp.ViewModel
{
    /// <summary>
    /// View model for search results page with AI and keyword search tabs.
    /// </summary>
    public class SearchViewModel
    {
        /// <summary>
        /// The search query entered by user
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// AI-matched suppliers with scores (Tab 1)
        /// </summary>
        public List<SupplierMatchResult> AiSuppliers { get; set; } = new List<SupplierMatchResult>();

        /// <summary>
        /// Keyword-matched products (Tab 2)
        /// </summary>
        public List<ProductSearchItem> Products { get; set; } = new List<ProductSearchItem>();

        /// <summary>
        /// Keyword-matched suppliers (Tab 2)
        /// </summary>
        public List<SupplierSearchItem> Suppliers { get; set; } = new List<SupplierSearchItem>();

        /// <summary>
        /// Whether AI search is enabled (query length >= 5)
        /// </summary>
        public bool IsAiSearchEnabled => !string.IsNullOrWhiteSpace(Query) && Query.Length >= 5;
    }

    /// <summary>
    /// Product item for keyword search results
    /// </summary>
    public class ProductSearchItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public DateTime? Created { get; set; }
    }

    /// <summary>
    /// Supplier item for keyword search results
    /// </summary>
    public class SupplierSearchItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int ProductCount { get; set; }
        public int Viewed { get; set; }
    }
}
