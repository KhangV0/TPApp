using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Data.Entities
{
    [Table("SearchIndexContents")]
    public class SearchIndexContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(1000)]
        public string? Title { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Contents { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? FutherIndex { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? RemovedUnicode { get; set; }

        public long? RefId { get; set; }

        [MaxLength(500)]
        public string? ImgPreview { get; set; }

        [MaxLength(100)]
        public string? TypeName { get; set; }

        [MaxLength(50)]
        public string? MimeType { get; set; }

        [MaxLength(500)]
        public string? URL { get; set; }

        [MaxLength(200)]
        public string? AbsPath { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        public DateTime? IndexTime { get; set; }

        [MaxLength(500)]
        public string? Noted { get; set; }

        [MaxLength(50)]
        public string? Creator { get; set; }

        [MaxLength(50)]
        public string? LanguageId { get; set; }

        public int? SiteId { get; set; }
    }
}
