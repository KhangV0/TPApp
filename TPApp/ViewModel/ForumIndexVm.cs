using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class ForumIndexVm
    {
        // ===== ROUTE PARAM =====
        public int? LinhVuc { get; set; }
        public int? ParentId { get; set; }

        // ===== PAGE INFO =====
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecord { get; set; }

        // ===== UI =====
        public string Title { get; set; } = "";
        public string TotalText { get; set; } = "";

        // ===== DATA =====
        public List<Category> Categories { get; set; } = new();

        // CNTB
        public List<ForumItemVm> CNTBItems { get; set; } = new();

        // DVTV
        public List<ForumItemVm> DVItems { get; set; } = new();

        // ===== PAGER =====
        public PagerVm Pager { get; set; } = new();

        public ForumPortletNhieuNhatVm PortletNhieunhat { get; set; }
        public ForumPortletDangMoVm PortletDangMo { get; set; }
        public ForumPortletTinTucVm PortletTinTuc { get; set; }
        public ForumPortletGiaiPhapCongNgheVm PortletGiaiPhapCongNghe { get; set; }

        
    }

    public class ForumItemVm
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Url { get; set; } = "";

        // "<b>user</b> lúc <i>date</i>"
        public string AuthorInfo { get; set; } = "";

        public int Viewed { get; set; }
        public int Comment { get; set; }
        public int Like { get; set; }

        // Category gắn kèm bài viết
        public List<CategoryVm> Categories { get; set; } = new();
    }

    public class CategoryVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class PagerVm
    {

        public int TotalRecord { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }

        // danh sách số trang để foreach
        public List<int> Pages { get; set; } = new();
    }
}
