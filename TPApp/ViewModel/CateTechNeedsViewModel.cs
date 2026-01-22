using Microsoft.AspNetCore.Mvc.Rendering;

namespace TPApp.ViewModel
{
    public class CateTechNeedsViewModel
    {
        public int MenuId { get; set; }

        public string? SelectedLinhVuc { get; set; }

        public List<SelectListItem> LinhVucs { get; set; } = new();

        public TechNeedItemVm? FirstItem { get; set; }

        public List<TechNeedItemVm> Items { get; set; } = new();

        // paging
        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
        public List<int> Pages { get; set; } = new();
        public PhieuYeuCauCNViewModel PhieuYeuCau { get; set; } = new();
        public PortletYeuCauMoiViewModel YeuCauMoi { get; set; } = new();

    }

    public class TechNeedItemVm
    {
        public int MenuId { get; set; }
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? QueryString { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public DateTime PublishedDate { get; set; }

        public string DetailUrl { get; set; } = "";
    }
}
