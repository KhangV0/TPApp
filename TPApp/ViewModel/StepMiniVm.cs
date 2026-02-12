namespace TPApp.ViewModel
{
    /// <summary>
    /// Minimal step information for workflow progress visualization
    /// </summary>
    public class StepMiniVm
    {
        public int StepNumber { get; set; } // 1 to 11
        public int StatusId { get; set; } // 0=NotStarted, 1=InProgress, 2=Completed
    }
}
