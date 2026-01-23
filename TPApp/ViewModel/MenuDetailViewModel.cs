namespace TPApp.ViewModel
{
    public class MenuDetailViewModel
    {
        public int MenuId { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Header { get; set; }

        public List<MenuItemViewModel> Menus { get; set; } = new();
    }

    public class MenuItemViewModel
    {
        public int MenuId { get; set; }
        public string? Title { get; set; }
        public string? NavigateUrl { get; set; }
    }

    public class MenuLeftViewModel
    {
        public int MenuId { get; set; }
        public string? Header { get; set; }
        public List<MenuItemViewModel> Menus { get; set; } = new();

    }

}
