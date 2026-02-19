namespace TPApp.Enums
{
    public enum LegalReviewStatus
    {
        /// <summary>Contract draft created, not yet submitted for review</summary>
        Draft           = 1,

        /// <summary>Submitted to legal reviewer, under review</summary>
        UnderReview     = 2,

        /// <summary>Reviewer requested changes</summary>
        ChangesRequested = 3,

        /// <summary>Reviewer approved the contract</summary>
        Approved        = 4,

        /// <summary>Both parties confirmed — Step 6 complete</summary>
        Completed       = 5
    }
}
