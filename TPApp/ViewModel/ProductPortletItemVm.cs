namespace TPApp.ViewModel
{
    public class ProductPortletItemVm
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? ImageUrl { get; set; }
        public string? Url { get; set; }
        public string? PriceText { get; set; }
        public int Star { get; set; }
        public bool IsSC { get; set; }
        public bool IsNC { get; set; }
    }
}
