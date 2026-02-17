using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ProjectAccessLogs")]
    public class ProjectAccessLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = null!;  // "ViewProject", "AcceptInvitation", "SubmitProposal", etc.

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(1000)]
        public string? AdditionalData { get; set; }  // JSON for extra context

        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
