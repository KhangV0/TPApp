namespace TPApp.Enums
{
    /// <summary>
    /// Represents the status of a workflow step
    /// Maps to ProjectSteps.StatusId and ProjectStepStates.Status columns
    /// </summary>
    public enum StepStatus
    {
        /// <summary>
        /// Step has not been started yet
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// Step is currently in progress
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Step has been submitted for review/approval
        /// </summary>
        Submitted = 2,

        /// <summary>
        /// Step has been approved (if approval required)
        /// </summary>
        Approved = 3,

        /// <summary>
        /// Step has been rejected (needs rework)
        /// </summary>
        Rejected = 4,

        /// <summary>
        /// Step has been completed successfully
        /// BREAKING CHANGE: Changed from value 2 to 5
        /// </summary>
        Completed = 5,

        /// <summary>
        /// Step is blocked (cannot proceed until prerequisite met)
        /// </summary>
        Blocked = 6
    }
}
