namespace TPApp.ViewModel
{
    public class ForumPortletGiaiPhapCongNgheVm
    {
        public List<ForumGiaiPhapCongNgheItemVm> Items { get; set; } = new();
    }

    public class ForumGiaiPhapCongNgheItemVm
    {
        public int MenuId { get; set; }
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? QueryString { get; set; }
        public string? ImageUrl { get; set; }
        public string? Tooltip { get; set; }
        public string? DateText { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
    }
}
