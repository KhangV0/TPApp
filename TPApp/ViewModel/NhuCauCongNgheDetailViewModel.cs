using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class NhuCauCongNgheDetailViewModel
    {
        public ContentYeucauDetailVm? Detail { get; set; }

        public List<RelationItemVm> Relations { get; set; } = new();

        public List<Album> Images { get; set; } = new();

        public int TargetId { get; set; }

        public int? CommentTypeId { get; set; }
        public PhieuYeuCauCNViewModel PhieuYeuCau { get; set; } = new();

        public PortletYeuCauMoiViewModel YeuCauMoi { get; set; } = new();
    }

    public class ContentYeucauDetailVm
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Contents { get; set; }
        public string? Author { get; set; }
        public string? QueryString { get; set; }
        public int MenuId { get; set; }
        public int? Viewed { get; set; }
        public int? Like { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int TypeId { get; set; }
    }
    public class RelationItemVm
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? QueryString { get; set; }

        public int MenuId { get; set; }

        public DateTime? PublishedDate { get; set; }
    }

}
