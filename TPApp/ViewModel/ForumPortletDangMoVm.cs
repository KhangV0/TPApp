namespace TPApp.ViewModel
{
    public class ForumPortletDangMoVm
    {
        public List<ForumPortletDangMoItemVm> Items { get; set; } = new();
    }

    public class ForumPortletDangMoItemVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Tooltip { get; set; } = "";
        public string DateText { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
