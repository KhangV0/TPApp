namespace TPApp.ViewModel
{
    public class TimKiemDoiTacIndexVm
    {
        public List<TimKiemDoiTacCategoryVm> Categories { get; set; } = new();
    }

    public class TimKiemDoiTacCategoryVm
    {
        public int CatId { get; set; }
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public List<TimKiemDoiTacItemVm> Products { get; set; } = new();
    }

    public class TimKiemDoiTacItemVm
    {
        public int TimDoiTacId { get; set; }
        public string TenSanPham { get; set; } = "";
        public string FullName { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Url { get; set; } = "";
        public int? Rating { get; set; }
    }

    public class TimKiemDoiTacDetailVm
    {
        public int TimDoiTacId { get; set; }

        public string TenSanPham { get; set; } = "";
        public string TenDonVi { get; set; } = "";
        public string FullName { get; set; } = "";

        public string DiaChi { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";

        public string MoTa { get; set; } = "";
        public string HinhThuc { get; set; } = "";

        public int? Rating { get; set; }
        public int Viewed { get; set; }

        public string? CategoryId { get; set; }

        public string ImageUrl { get; set; } = "";

        // ====== LEFT MENU ======
        public List<TimKiemDoiTacCategoryVm> Categories { get; set; } = new();

        // ====== RELATED ITEMS ======
        public List<TimKiemDoiTacItemVm> RelatedItems { get; set; } = new();
    }
    public class TimKiemDoiTacListVm
    {
        public int CateId { get; set; }
        public string CateTitle { get; set; } = "";

        public string? CategoryIds { get; set; }   
        public int StoreId { get; set; }

        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalPages =>
            PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

        public List<TimKiemDoiTacItemVm> Items { get; set; } = new();
        public List<TimKiemDoiTacCategoryVm> Categories { get; set; } = new();
    }
}
