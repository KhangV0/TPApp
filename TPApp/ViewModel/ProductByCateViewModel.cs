using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class ProductByCateViewModel
    {
        public string CateTitle { get; set; }
        public string PageTitle { get; set; }

        public List<CategoryItemVm> Categories { get; set; } = new();
        public List<ProductItemVm> Products { get; set; } = new();
        public List<PageItemVm> Pages { get; set; } = new();

        public int PageSize { get; set; }
        public int Total { get; set; }

        public int CurPage { get; set; }
        public int CateId { get; set; }
        public int StoreId { get; set; }
    }
    public class CategoryItemVm
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
    public class ProductItemVm
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string PriceText { get; set; }
        public string Code { get; set; }
        public int Star { get; set; }

        public bool IsSC { get; set; }
        public bool IsNC { get; set; }
    }
    public class PageItemVm
    {
        public int Page { get; set; }
        public bool IsActive { get; set; }
    }

}
