namespace TPApp.ViewModel
{
    public class StepContentVm
    {
        public int ProjectId { get; set; }
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public TPApp.Entities.Project Project { get; set; } = null!;
        public int UserRole { get; set; }
        public List<ProjectStepNavVm> Steps { get; set; } = new();
        public int CurrentStep { get; set; }
        
        // Step-specific data (populated based on step)
        public object? StepData { get; set; }
    }
}
