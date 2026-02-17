namespace TPApp.Enums
{
    /// <summary>
    /// Represents the 14 workflow steps in technology transfer process
    /// </summary>
    public enum WorkflowStep
    {
        /// <summary>
        /// Step 1: Technology Transfer Request
        /// </summary>
        Request = 1,

        /// <summary>
        /// Step 2: NDA Agreement (E-Sign required)
        /// </summary>
        NDA = 2,

        /// <summary>
        /// Step 3: Request for Quotation
        /// </summary>
        RFQ = 3,

        /// <summary>
        /// Step 4: Proposal Submission
        /// </summary>
        Proposal = 4,

        /// <summary>
        /// Step 5: Commercial Negotiation
        /// </summary>
        Negotiation = 5,

        /// <summary>
        /// Step 6: Legal Review
        /// </summary>
        LegalReview = 6,

        /// <summary>
        /// Step 7: E-Contract Signing
        /// </summary>
        EContract = 7,

        /// <summary>
        /// Step 8: Advance Payment Confirmation
        /// </summary>
        AdvancePayment = 8,

        /// <summary>
        /// Step 9: Pilot Testing
        /// </summary>
        PilotTest = 9,

        /// <summary>
        /// Step 10: Equipment Handover & Deployment
        /// </summary>
        Handover = 10,

        /// <summary>
        /// Step 11: Training & Operational Handover
        /// </summary>
        Training = 11,

        /// <summary>
        /// Step 12: Technical Documentation Handover
        /// </summary>
        TechnicalDocs = 12,

        /// <summary>
        /// Step 13: Acceptance Testing
        /// </summary>
        Acceptance = 13,

        /// <summary>
        /// Step 14: Contract Liquidation
        /// </summary>
        Liquidation = 14
    }
}
