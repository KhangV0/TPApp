using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ContractSignatureRequests")]
    public class ContractSignatureRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ContractId { get; set; }

        public int UserId { get; set; }

        /// <summary>1=Buyer, 2=Seller</summary>
        public int Role { get; set; }

        /// <summary>See ContractSignatureType enum</summary>
        public int SignatureType { get; set; }

        [StringLength(50)]
        public string? Provider { get; set; } // VNPT/FPT/Viettel/Local

        /// <summary>See ContractSignatureStatus enum</summary>
        public int StatusId { get; set; } = 0; // Pending

        [StringLength(200)]
        public string? RequestRef { get; set; } // Transaction ID from CA provider

        [StringLength(200)]
        public string? ChallengeRef { get; set; } // OTP session key

        [StringLength(200)]
        public string? CallbackSecret { get; set; }

        [StringLength(100)]
        public string? ErrorCode { get; set; }

        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}
