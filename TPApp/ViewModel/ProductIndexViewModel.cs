using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class ProductIndexViewModel
    {
        public List<SanPhamCNTB> NewProducts { get; set; } = new();
        public List<CategoryBlockVm> Categories { get; set; } = new();
    }

    public class CategoryBlockVm
    {
        public Category Category { get; set; }
        public List<SanPhamCNTB> Products { get; set; } = new();
    }

    public class ProductRelatedItemVm
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? PriceText { get; set; }
        public string? Url { get; set; }
        public int Star { get; set; }
    }
}
