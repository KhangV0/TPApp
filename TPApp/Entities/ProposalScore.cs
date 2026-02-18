using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ProposalScores")]
    public class ProposalScore
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProposalId { get; set; }

        [Required]
        public int ConsultantId { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal? TechnicalScore { get; set; } // 0.0 to 10.0

        [Column(TypeName = "decimal(3,1)")]
        public decimal? PriceScore { get; set; } // 0.0 to 10.0

        [Column(TypeName = "decimal(3,1)")]
        public decimal? TimelineScore { get; set; } // 0.0 to 10.0

        [Column(TypeName = "decimal(3,1)")]
        public decimal? OverallScore { get; set; } // 0.0 to 10.0

        public string? Comments { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("ProposalId")]
        public ProposalSubmission? Proposal { get; set; }

        [ForeignKey("ConsultantId")]
        public ApplicationUser? Consultant { get; set; }
    }
}
