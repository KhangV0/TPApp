using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities.ESign
{
    /// <summary>
    /// Represents a signature on an electronic document
    /// Tracks OTP verification and signature hash
    /// </summary>
    [Table("ESignSignatures")]
    public class ESignSignature
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Reference to the document being signed
        /// </summary>
        [Required]
        public long DocumentId { get; set; }

        /// <summary>
        /// User who is signing this document
        /// </summary>
        [Required]
        public int SignerUserId { get; set; }

        /// <summary>
        /// Role of the signer: Buyer, Seller, Platform
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SignerRole { get; set; } = null!;

        /// <summary>
        /// SHA256 hash of the signature data for verification
        /// </summary>
        [StringLength(64)]
        public string? SignatureHash { get; set; }

        /// <summary>
        /// OTP code sent to user (hashed for security)
        /// </summary>
        [StringLength(100)]
        public string? OtpCodeHash { get; set; }

        /// <summary>
        /// When OTP was sent
        /// </summary>
        public DateTime? OtpSentAt { get; set; }

        /// <summary>
        /// When OTP was verified
        /// </summary>
        public DateTime? OtpVerifiedAt { get; set; }

        /// <summary>
        /// Signature status: 0=Pending, 1=Signed, 2=Rejected
        /// </summary>
        [Required]
        public int Status { get; set; } = 0;

        /// <summary>
        /// When this signature record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the document was actually signed
        /// </summary>
        public DateTime? SignedAt { get; set; }

        /// <summary>
        /// IP address of the signer
        /// </summary>
        [StringLength(64)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent (browser) of the signer
        /// </summary>
        [StringLength(300)]
        public string? UserAgent { get; set; }

        // Navigation properties
        [ForeignKey("DocumentId")]
        public virtual ESignDocument? Document { get; set; }
    }
}
