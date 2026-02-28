using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    /// <summary>Maps to existing [dbo].[Feedback] table in SanGDCloud DB.</summary>
    [Table("Feedback")]
    public class Feedback
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [MaxLength(200)]
        public string? FullName { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>Subject / Tiêu đề.</summary>
        [MaxLength(300)]
        public string? Title { get; set; }

        /// <summary>Message / Nội dung.</summary>
        public string? Content { get; set; }

        [MaxLength(50)]
        public string? Creator { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        [MaxLength(50)]
        public string? Modifier { get; set; }

        public int? StatusId { get; set; }

        public DateTime? PublishedDate { get; set; }
        public DateTime? bEffectiveDate { get; set; }
        public DateTime? eEffectiveDate { get; set; }

        [MaxLength(500)]
        public string? Domain { get; set; }

        public int? LanguageId { get; set; }
        public int? DeptId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }
}
