namespace TPApp.Services
{
    /// <summary>
    /// Internal DTO for step data used in DashboardService
    /// </summary>
    internal class StepDto
    {
        public int ProjectId { get; set; }
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StatusId { get; set; }
    }
}
