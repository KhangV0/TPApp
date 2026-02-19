using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ContractApprovals")]
    public class ContractApproval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ContractId { get; set; }

        public int UserId { get; set; }

        /// <summary>1=Buyer, 2=Seller, 3=Consultant</summary>
        public int Role { get; set; }

        /// <summary>See ContractApprovalStatus enum: 0=Pending, 1=Approved, 2=Rejected</summary>
        public int StatusId { get; set; } = 0;

        public string? Comment { get; set; }

        public DateTime? DecisionAt { get; set; }

        [StringLength(100)]
        public string? IPAddress { get; set; }

        [StringLength(400)]
        public string? UserAgent { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
