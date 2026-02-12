using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ProjectMembers")]
    public class ProjectMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        [Required]
        public int UserId { get; set; } // int to match database schema

        // 1=Buyer, 2=Seller, 3=Consultant
        public int Role { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
