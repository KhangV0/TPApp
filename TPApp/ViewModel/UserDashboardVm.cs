namespace TPApp.ViewModel
{
    /// <summary>
    /// Main dashboard view model containing user info, statistics, and project list
    /// </summary>
    public class UserDashboardVm
    {
        // User information
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        // Statistics
        public int TotalProjects { get; set; }
        public int InProgressProjects { get; set; }
        public int WaitingForMe { get; set; }
        public int CompletedProjects { get; set; }
        
        // Project list
        public List<ProjectDashboardItemVm> Projects { get; set; } = new List<ProjectDashboardItemVm>();
    }
}
