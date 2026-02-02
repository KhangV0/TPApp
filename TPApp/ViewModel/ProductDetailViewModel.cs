using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class ProductDetailViewModel
    {
        public SanPhamCNTB Product { get; set; }

        public string CategoryTitle { get; set; }
        
        // Ensure these properties are explicitly public and available
        public string SupplierName { get; set; }
        public string SupplierUrl { get; set; }

        public List<Category> Industries { get; set; }
        public List<NhaCungUng> Suppliers { get; set; }

        public List<VSImage> Images { get; set; }
        public List<Category> RelatedCategories { get; set; }

        public List<KeywordVm> Keywords { get; set; }
       
        // Note: RelatedProducts type might have been causing issues if mismatched
        public List<ProductRelatedItemVm> RelatedProducts { get; set; }

        public int RatingCount { get; set; }
        public int Viewed { get; set; }

        // Video
        public bool IsYoutube { get; set; }
        public string YoutubeEmbedUrl { get; set; }
        public string VideoFileUrl { get; set; }

        // UI flags
        public int TypeId { get; set; }
    }

    public class KeywordVm
    {
        public int KeywordId { get; set; }
        public string Keyword { get; set; }
    }
}
