using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPApp.Enums;

namespace TPApp.Entities
{
    [Table("ContractComments")]
    public class ContractComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public int LegalReviewFormId { get; set; }

        [Required]
        public string CommentText { get; set; } = string.Empty;

        /// <summary>ContractCommentType enum value</summary>
        public int CommentType { get; set; } = (int)ContractCommentType.General;

        public bool IsResolved { get; set; } = false;

        public int AuthorId { get; set; }

        [StringLength(200)]
        public string? AuthorName { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public DateTime? NgaySua { get; set; }
    }
}
