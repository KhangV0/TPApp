using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class ProductIndexViewModel
    {
        public string? ProductCNMoiCapNhatHtml { get; set; }
        public List<CategoryBlockVm> Categories { get; set; } = new();
    }

    public class CategoryBlockVm
    {
        public Category Category { get; set; }
        public List<SanPhamCNTB> Products { get; set; } = new();
    }
}
