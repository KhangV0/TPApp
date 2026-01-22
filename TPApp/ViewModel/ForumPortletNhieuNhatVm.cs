namespace TPApp.ViewModel
{
    public class ForumPortletNhieuNhatVm
    {
        public int ActiveTab { get; set; } = 1;
        public List<ForumHoiNhieuVm> HoiNhieu { get; set; } = new();
        public List<ForumTraLoiNhieuVm> TraLoiNhieu { get; set; } = new();
    }

    public class ForumHoiNhieuVm
    {
        public string FullName { get; set; }
        public int CountyCTB { get; set; }
    }

    public class ForumTraLoiNhieuVm
    {
        public string Name { get; set; }
        public int CountyCTB { get; set; }
    }
}
