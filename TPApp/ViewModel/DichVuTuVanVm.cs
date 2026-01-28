using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class DichVuTuVanIndexVm {
        public DichVuTuVanVm DichVuTuVan { get; set; }
    }

    public class DichVuTuVanVm
    {
        public int MenuId { get; set; }
        public int? SelectedCateId { get; set; }

        public string MainDomain { get; set; } = "";

        public List<SelectItemVm> DichVuOptions { get; set; } = new();

        public List<NhaTuVan> Items { get; set; } = new();
        public List<NhaCungUng> ItemsCungUng { get; set; } = new();

        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public List<int> Pages { get; set; } = new();

        public int TotalPage =>
            PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }


    public class SelectItemVm
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class DichVuTuVanListVm
    {
        public string? Title { get; set; }
        public string? TitleCU { get; set; }

        public List<NhaTuVanItemVm> TuVanItems { get; set; } = new();
        public List<NhaTuVanItemVm> NhaCungUngItems { get; set; } = new();

        public List<CategoryVm> Categories { get; set; } = new();

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }

        public int PageCU { get; set; }
        public int PageSizeCU { get; set; }
        public int TotalCU { get; set; }
    }

}
