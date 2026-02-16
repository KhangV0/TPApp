using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Data.Entities
{
    /// <summary>
    /// Search query log for analytics and trending searches
    /// </summary>
    [Table("SearchQueryLog")]
    public class SearchQueryLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Keyword { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string NormalizedKeyword { get; set; } = string.Empty;

        public int ResultCount { get; set; }

        [Required]
        [MaxLength(20)]
        public string SearchMode { get; set; } = "normal"; // 'normal' or 'ai'

        [MaxLength(50)]
        public string? LanguageId { get; set; }

        [MaxLength(100)]
        public string? TypeName { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        public int? ExecutionTimeMs { get; set; }
    }
}
