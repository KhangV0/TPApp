namespace TPApp.ViewModel
{
    /// <summary>
    /// Result of permission check for a specific step and user
    /// </summary>
    public class StepPermissionResult
    {
        /// <summary>
        /// User can view the step
        /// </summary>
        public bool CanView { get; set; }

        /// <summary>
        /// User can edit step data
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// User can submit the step for completion/approval
        /// </summary>
        public bool CanSubmit { get; set; }

        /// <summary>
        /// User can approve/reject step submissions
        /// </summary>
        public bool CanApprove { get; set; }

        /// <summary>
        /// Reason why access is blocked (if any)
        /// </summary>
        public string? BlockedReason { get; set; }

        /// <summary>
        /// Indicates if user is blocked from accessing this step
        /// </summary>
        public bool IsBlocked => !string.IsNullOrEmpty(BlockedReason);

        /// <summary>
        /// User has no permissions at all for this step
        /// </summary>
        public bool HasNoAccess => !CanView && !CanEdit && !CanSubmit && !CanApprove;

        /// <summary>
        /// Create a blocked result with reason
        /// </summary>
        public static StepPermissionResult Blocked(string reason)
        {
            return new StepPermissionResult
            {
                CanView = false,
                CanEdit = false,
                CanSubmit = false,
                CanApprove = false,
                BlockedReason = reason
            };
        }

        /// <summary>
        /// Create a result with full access
        /// </summary>
        public static StepPermissionResult FullAccess()
        {
            return new StepPermissionResult
            {
                CanView = true,
                CanEdit = true,
                CanSubmit = true,
                CanApprove = true
            };
        }
    }
}
