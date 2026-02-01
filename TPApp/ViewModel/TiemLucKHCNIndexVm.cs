using TPApp.Data.Entities;
namespace TPApp.ViewModel
{
    public class TiemLucKHCNIndexVm
    {
        public string? TypeKey { get; set; }
        public string? TypeName { get; set; }

        public string SearchText { get; set; } = "";

        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
        public int TotalRecord { get; set; }

        public List<SearchIndexContent> Items { get; set; } = new();
        public List<int> Pages { get; set; } = new();
    }
}
