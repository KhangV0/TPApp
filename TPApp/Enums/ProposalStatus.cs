namespace TPApp.Enums
{
    /// <summary>
    /// Status of proposal submissions
    /// </summary>
    public enum ProposalStatus
    {
        /// <summary>
        /// Draft - not yet submitted
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Submitted - waiting for buyer review
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Selected - chosen by buyer
        /// </summary>
        Selected = 2,

        /// <summary>
        /// Rejected - not selected
        /// </summary>
        Rejected = 3
    }
}
