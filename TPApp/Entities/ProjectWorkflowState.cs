using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    /// <summary>
    /// Single source of truth for workflow state per project
    /// Tracks overall workflow progress and current step
    /// </summary>
    [Table("ProjectWorkflowStates")]
    public class ProjectWorkflowState
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
        /// Current active step (1-14)
        /// </summary>
        [Required]
        public int CurrentStep { get; set; }

        /// <summary>
        /// Overall project workflow status
        /// 1=Active, 2=Completed, 3=Suspended, 4=Cancelled
        /// </summary>
        [Required]
        public int OverallStatus { get; set; } = 1;

        /// <summary>
        /// When this workflow state was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
    }
}
