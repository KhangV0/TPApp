using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    /// <summary>
    /// Tracks the status and evidence for each workflow step
    /// One record per project per step (14 records per project)
    /// </summary>
    [Table("ProjectStepStates")]
    public class ProjectStepState
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
        /// Step number (1-14)
        /// </summary>
        [Required]
        public int StepNumber { get; set; }

        /// <summary>
        /// Current status of this step (StepStatus enum)
        /// 0=NotStarted, 1=InProgress, 2=Submitted, 3=Approved, 4=Rejected, 5=Completed, 6=Blocked
        /// </summary>
        [Required]
        public int Status { get; set; } = 0;

        /// <summary>
        /// When this step was started
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// When this step was submitted for review
        /// </summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>
        /// When this step was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Reason why this step is blocked (if Status=Blocked)
        /// </summary>
        [StringLength(500)]
        public string? BlockedReason { get; set; }

        /// <summary>
        /// Reference to the data table for this step (e.g., "NDAAgreements", "EContracts")
        /// </summary>
        [StringLength(100)]
        public string? DataRefTable { get; set; }

        /// <summary>
        /// Reference to the specific record ID in the data table
        /// </summary>
        [StringLength(50)]
        public string? DataRefId { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
    }
}
