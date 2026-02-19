using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ContractSignatures")]
    public class ContractSignature
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ContractId { get; set; }

        public int SignatureRequestId { get; set; }

        public int UserId { get; set; }

        public int Role { get; set; }

        public int SignatureType { get; set; }

        [StringLength(50)]
        public string? Provider { get; set; }

        [StringLength(200)]
        public string? CertificateSerial { get; set; }

        [StringLength(500)]
        public string? CertificateSubject { get; set; }

        [StringLength(500)]
        public string? CertificateIssuer { get; set; }

        [StringLength(128)]
        public string? SignedHash { get; set; }

        public DateTime? SignedAt { get; set; }

        public string? TimeStampToken { get; set; }

        /// <summary>0=Unknown, 1=Valid, 2=Invalid</summary>
        public int VerificationStatus { get; set; } = 0;

        [StringLength(100)]
        public string? IPAddress { get; set; }

        [StringLength(400)]
        public string? UserAgent { get; set; }

        public string? RawProviderPayload { get; set; }
    }
}
