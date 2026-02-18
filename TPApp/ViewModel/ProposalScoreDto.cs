namespace TPApp.ViewModel
{
    public class ProposalScoreDto
    {
        public int ProposalId { get; set; }
        public int ProjectId { get; set; }
        public decimal TechnicalScore { get; set; } // 0.0 to 10.0
        public decimal PriceScore { get; set; } // 0.0 to 10.0
        public decimal TimelineScore { get; set; } // 0.0 to 10.0
        public decimal OverallScore { get; set; } // 0.0 to 10.0
        public string? Comments { get; set; }
    }
}
