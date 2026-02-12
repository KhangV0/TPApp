namespace TPApp.ViewModel
{
    public class ProjectStepNavVm
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public bool IsCurrent { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public bool IsAccessible { get; set; }
    }
}
