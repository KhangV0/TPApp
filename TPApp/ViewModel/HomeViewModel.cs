namespace TPApp.ViewModel
{
    public class HomeViewModel
    {
        public string? CongNgheMoiCapNhatHtml { get; set; }
        public string? ProductCNMoiCapNhatHtml { get; set; }

        public List<TinSuKienTabVm>? TinSuKien { get; set; }
        public List<VideoVm>? VideoCongNghe { get; set; }
        public YeuCauCongNgheVm? YeuCauCongNghe { get; set; }

        /// <summary>Enterprise analytics — from HomeAnalyticsService (cached, max 2 queries).</summary>
        public HomeAnalyticsVm Analytics { get; set; } = new();
    }
}
