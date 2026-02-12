namespace TPApp.Enums
{
    /// <summary>
    /// Represents the status of a workflow step
    /// Maps to ProjectSteps.StatusId column
    /// </summary>
    public enum StepStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
}
