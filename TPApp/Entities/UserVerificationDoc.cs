using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("UserVerificationDocs")]
    public class UserVerificationDoc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>1=CCCD_Front, 2=CCCD_Back, 3=BusinessLicense</summary>
        public int DocType { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = null!;

        [Required, MaxLength(200)]
        public string FileName { get; set; } = null!;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>0=Pending, 1=Approved, 2=Rejected</summary>
        public int ReviewStatus { get; set; } = 0;

        [MaxLength(500)]
        public string? ReviewNote { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }

    public static class DocType
    {
        public const int CccdFront       = 1;
        public const int CccdBack        = 2;
        public const int BusinessLicense = 3;
    }
}
