using TPApp.Entities;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Core workflow engine for managing state machine transitions
    /// Enforces guard policies and logs all transitions
    /// </summary>
    public interface IWorkflowEngine
    {
        /// <summary>
        /// Check if a user can start a specific step
        /// </summary>
        Task<bool> CanStartStepAsync(int projectId, int stepNumber, int userId);

        /// <summary>
        /// Get the reason why a step is blocked (if blocked)
        /// </summary>
        Task<string?> GetBlockedReasonAsync(int projectId, int stepNumber);

        /// <summary>
        /// Start a workflow step (transition to InProgress)
        /// </summary>
        Task StartStepAsync(int projectId, int stepNumber, int userId, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Submit a step for review/approval (transition to Submitted)
        /// </summary>
        Task SubmitStepAsync(int projectId, int stepNumber, int userId, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Approve a submitted step (transition to Approved)
        /// </summary>
        Task ApproveStepAsync(int projectId, int stepNumber, int userId, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Reject a submitted step (transition to Rejected)
        /// </summary>
        Task RejectStepAsync(int projectId, int stepNumber, int userId, string reason, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Complete a workflow step (transition to Completed, unlock next step)
        /// </summary>
        Task CompleteStepAsync(int projectId, int stepNumber, int userId, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Get the overall workflow state for a project
        /// </summary>
        Task<ProjectWorkflowState?> GetWorkflowStateAsync(int projectId);

        /// <summary>
        /// Get all step states for a project (14 steps)
        /// </summary>
        Task<List<ProjectStepState>> GetStepStatesAsync(int projectId);

        /// <summary>
        /// Get the current active step number
        /// </summary>
        Task<int> GetCurrentStepAsync(int projectId);

        /// <summary>
        /// Initialize workflow state for a new project
        /// </summary>
        Task InitializeWorkflowAsync(int projectId, int userId);
    }
}
