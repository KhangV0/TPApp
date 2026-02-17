using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities.ESign
{
    /// <summary>
    /// Represents an electronic document that requires signatures
    /// Used for NDA, E-Contracts, and other legal documents
    /// </summary>
    [Table("ESignDocuments")]
    public class ESignDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Reference to the project
        /// </summary>
        [Required]
        public int ProjectId { get; set; }

        /// <summary>
        /// Document type: 1=ProjectNDA, 2=EContract, 3=Other
        /// </summary>
        [Required]
        public int DocType { get; set; }

        /// <summary>
        /// Document name/title
        /// </summary>
        [Required]
        [StringLength(200)]
        public string DocumentName { get; set; } = null!;

        /// <summary>
        /// Path to the uploaded document file
        /// </summary>
        [StringLength(500)]
        public string? FilePath { get; set; }

        /// <summary>
        /// SHA256 hash of the document for integrity verification
        /// </summary>
        [StringLength(64)]
        public string? FileHash { get; set; }

        /// <summary>
        /// Document status: 0=Draft, 1=Pending, 2=Signed, 3=Rejected
        /// </summary>
        [Required]
        public int Status { get; set; } = 0;

        /// <summary>
        /// When this document was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When all required signatures were completed
        /// </summary>
        public DateTime? SignedAt { get; set; }

        /// <summary>
        /// User who created this document
        /// </summary>
        public int? CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }

        public virtual ICollection<ESignSignature>? Signatures { get; set; }
    }
}
