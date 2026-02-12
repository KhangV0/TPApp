namespace TPApp.ViewModel
{
    /// <summary>
    /// Individual project item for dashboard table
    /// </summary>
    public class ProjectDashboardItemVm
    {
        public int ProjectId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty; // Buyer/Seller/Consultant
        
        // Current step information
        public int CurrentStepNumber { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public string CurrentStepStatus { get; set; } = string.Empty; // NotStarted/InProgress/Completed
        
        // Progress tracking
        public int CompletedSteps { get; set; }
        public int ProgressPercent { get; set; }
        
        // Workflow visualization (11 steps)
        public List<StepMiniVm> StepsSummary { get; set; } = new List<StepMiniVm>();
    }
}
