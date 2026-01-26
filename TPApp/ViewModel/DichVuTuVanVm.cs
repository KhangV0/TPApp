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
}
