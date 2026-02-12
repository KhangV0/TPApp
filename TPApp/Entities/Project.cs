using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ProjectCode { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public int StatusId { get; set; } = 1; // 1=Draft, 2=Active, 3=Completed

        public int? CreatedBy { get; set; } // int to match database UserId

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? ModifiedBy { get; set; } // int to match database UserId
        
        public DateTime? ModifiedDate { get; set; }
    }
}
