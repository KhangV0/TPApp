using Microsoft.AspNetCore.Mvc.Rendering;
using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class DichVuCungUngVm
    {
        public int SelectedIndustryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 16;
        public int TotalRecord { get; set; }

        public List<SelectListItem> Industries { get; set; } = new();
        public List<NhaCungUng> Items { get; set; } = new();

        public int TotalPage =>
            (int)Math.Ceiling((double)TotalRecord / PageSize);
    }

}
