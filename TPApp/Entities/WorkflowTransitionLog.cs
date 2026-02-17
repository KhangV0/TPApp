using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    /// <summary>
    /// Immutable audit log for all workflow transitions
    /// Provides complete history of workflow state changes
    /// </summary>
    [Table("WorkflowTransitionLogs")]
    public class WorkflowTransitionLog
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
        /// Step number before transition (null if starting workflow)
        /// </summary>
        public int? FromStep { get; set; }

        /// <summary>
        /// Step number after transition
        /// </summary>
        [Required]
        public int ToStep { get; set; }

        /// <summary>
        /// Action performed: Start, Submit, Approve, Reject, Complete
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = null!;

        /// <summary>
        /// User who performed this action
        /// </summary>
        [Required]
        public int ActorUserId { get; set; }

        /// <summary>
        /// IP address of the actor
        /// </summary>
        [StringLength(64)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent (browser) of the actor
        /// </summary>
        [StringLength(300)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Additional message or context for this transition
        /// </summary>
        [StringLength(1000)]
        public string? Message { get; set; }

        /// <summary>
        /// When this transition occurred (immutable)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
    }
}
