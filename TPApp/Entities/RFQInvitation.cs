using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("RFQInvitations")]
    public class RFQInvitation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int RFQId { get; set; }

        [Required]
        public int SellerId { get; set; }  // UserId of invited seller

        [Required]
        public DateTime InvitedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Status: 0=Invited, 1=Viewed, 2=Accepted, 3=Declined
        /// </summary>
        [Required]
        public int StatusId { get; set; } = 0;

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime? ViewedDate { get; set; }

        public DateTime? ResponseDate { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }

        [ForeignKey("RFQId")]
        public virtual RFQRequest? RFQRequest { get; set; }

        [ForeignKey("SellerId")]
        public virtual ApplicationUser? Seller { get; set; }
    }
}
