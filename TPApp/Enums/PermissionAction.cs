namespace TPApp.Enums
{
    /// <summary>
    /// Permission actions that can be performed on workflow steps
    /// </summary>
    public enum PermissionAction
    {
        /// <summary>
        /// View/Read access to step data
        /// </summary>
        View = 1,

        /// <summary>
        /// Edit/Modify step data
        /// </summary>
        Edit = 2,

        /// <summary>
        /// Submit step for completion or approval
        /// </summary>
        Submit = 3,

        /// <summary>
        /// Approve/Reject step submissions
        /// </summary>
        Approve = 4
    }
}
