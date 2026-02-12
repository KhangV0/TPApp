using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class ProjectDetailWithStepsVm
    {
        public Project Project { get; set; } = null!;
        public List<ProjectStepNavVm> Steps { get; set; } = new();
        public int CurrentStep { get; set; }
        public int UserRole { get; set; }
        public int ProgressPercent { get; set; }
    }
}
