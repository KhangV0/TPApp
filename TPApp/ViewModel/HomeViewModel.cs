namespace TPApp.ViewModel
{
    public class HomeViewModel
    {
        public string? CongNgheMoiCapNhatHtml { get; set; }
        public string? ProductCNMoiCapNhatHtml { get; set; }
                        
        public List<TinSuKienTabVm>? TinSuKien { get; set; }
        public List<VideoVm>? VideoCongNghe { get; set; }
        public YeuCauCongNgheVm? YeuCauCongNghe { get; set; }
    }
}
