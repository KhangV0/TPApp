using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class ProposalListVm
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public List<ProposalItemVm> Proposals { get; set; } = new();
        public bool IsOwner { get; set; }
        public bool IsConsultant { get; set; }
        public int? SelectedProposalId { get; set; }
    }

    public class ProposalItemVm
    {
        public ProposalSubmission Proposal { get; set; } = null!;
        public string SellerName { get; set; } = string.Empty;
        public List<ProposalScore> Scores { get; set; } = new();
        public decimal AverageScore { get; set; }
        public bool IsSelected { get; set; }
    }
}
