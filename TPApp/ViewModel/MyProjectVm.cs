namespace TPApp.ViewModel
{
    public class MyProjectVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int RoleId { get; set; }
        public int CurrentStep { get; set; }
        public string Status { get; set; } = null!;
        public int StatusId { get; set; }
        public int ProgressPercent { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
