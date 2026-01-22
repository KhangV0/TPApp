using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Keyword")]
    public class KeywordEntity
    {
        [Key]
        public long KeywordID { get; set; }

        [MaxLength(500)]
        public string? Keyword { get; set; }

        public int? Viewed { get; set; }

        [MaxLength(500)]
        public string? QueryString { get; set; }

        [Required]
        [MaxLength(500)]
        public string Domain { get; set; } = string.Empty;

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
