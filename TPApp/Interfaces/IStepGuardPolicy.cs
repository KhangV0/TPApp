namespace TPApp.Interfaces
{
    /// <summary>
    /// Guard policy for controlling access to workflow steps
    /// Enforces prerequisites and business rules
    /// </summary>
    public interface IStepGuardPolicy
    {
        /// <summary>
        /// Check if a user can access a specific step
        /// Returns true if accessible, false if blocked
        /// </summary>
        Task<bool> CanAccessStepAsync(int projectId, int stepNumber, int userId);

        /// <summary>
        /// Get the reason why a step is blocked
        /// Returns null if step is accessible
        /// </summary>
        Task<string?> GetBlockedReasonAsync(int projectId, int stepNumber);

        /// <summary>
        /// Check if a step can be completed
        /// Some steps have special completion requirements (e.g., Step 2 needs E-Sign)
        /// </summary>
        Task<bool> CanCompleteStepAsync(int projectId, int stepNumber, int userId);

        /// <summary>
        /// Get the reason why a step cannot be completed
        /// Returns null if step can be completed
        /// </summary>
        Task<string?> GetCompletionBlockedReasonAsync(int projectId, int stepNumber);
    }
}
