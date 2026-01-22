using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class PortletYeuCauMoiViewModel
    {
        public string Header { get; set; } = "Yêu cầu mới";
        public List<PhieuYeuCauCNTB> Items { get; set; } = new();
    }
}
