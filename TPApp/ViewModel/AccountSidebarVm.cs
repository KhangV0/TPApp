namespace TPApp.ViewModel
{
    public class AccountSidebarVm
    {
        public string FullName { get; set; } = "User";
        public string Email { get; set; } = "";
        public string AvatarUrl { get; set; } = "/images/default-avatar.png";
        public int ProjectCount { get; set; }
        public int InvitationCount { get; set; }
        public bool IsSeller { get; set; }
    }
}
