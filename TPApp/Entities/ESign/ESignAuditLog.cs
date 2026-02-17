using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities.ESign
{
    /// <summary>
    /// Immutable audit log for all E-Sign activities
    /// Provides complete traceability for compliance
    /// </summary>
    [Table("ESignAuditLogs")]
    public class ESignAuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Reference to the document
        /// </summary>
        [Required]
        public long DocumentId { get; set; }

        /// <summary>
        /// User who performed this action
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Action performed: Upload, SendOTP, VerifyOTP, Sign, Reject, Download
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = null!;

        /// <summary>
        /// IP address of the user
        /// </summary>
        [StringLength(64)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent (browser) of the user
        /// </summary>
        [StringLength(300)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Additional details about this action (JSON format)
        /// </summary>
        [StringLength(1000)]
        public string? Details { get; set; }

        /// <summary>
        /// When this action occurred (immutable)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("DocumentId")]
        public virtual ESignDocument? Document { get; set; }
    }
}
