using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class SellerInvitationVm
    {
        public RFQInvitation Invitation { get; set; } = null!;
        public Project Project { get; set; } = null!;
        public RFQRequest RFQ { get; set; } = null!;
        public bool NdaSigned { get; set; }
        public bool ProposalSubmitted { get; set; }
        public bool DeadlinePassed { get; set; }
        public int DaysUntilDeadline { get; set; }

        public string StatusText
        {
            get
            {
                if (ProposalSubmitted) return "Đã gửi báo giá";
                if (DeadlinePassed) return "Đã hết hạn";
                if (!NdaSigned) return "Chưa ký NDA";
                if (Invitation.StatusId == 0) return "Chờ phản hồi";
                if (Invitation.StatusId == 1) return "Đã xem";
                if (Invitation.StatusId == 2) return "Đã chấp nhận";
                if (Invitation.StatusId == 3) return "Đã từ chối";
                return "Không xác định";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (ProposalSubmitted) return "bg-success";
                if (DeadlinePassed) return "bg-danger";
                if (!NdaSigned) return "bg-warning";
                if (Invitation.StatusId == 2) return "bg-info";
                if (Invitation.StatusId == 3) return "bg-secondary";
                return "bg-primary";
            }
        }

        public bool CanSubmitProposal => NdaSigned && !DeadlinePassed && !ProposalSubmitted && Invitation.StatusId != 3;
    }
}
