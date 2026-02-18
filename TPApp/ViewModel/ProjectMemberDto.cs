namespace TPApp.ViewModel
{
    public class ProjectMemberDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserSelectDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
