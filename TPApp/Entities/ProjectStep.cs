using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ProjectSteps")]
    public class ProjectStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public int StepNumber { get; set; } // 1 to 11

        [StringLength(200)]
        public string StepName { get; set; } = null!;

        public int StatusId { get; set; } // 0=NotStarted, 1=InProgress, 2=Completed

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }
    }
}
