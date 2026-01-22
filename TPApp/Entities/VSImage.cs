using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("VSImage")]
    public class VSImage
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(300)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? FileURL { get; set; }

        public DateTime? Created { get; set; }

        [MaxLength(50)]
        public string? Creator { get; set; }

        public DateTime? Modified { get; set; }

        [MaxLength(50)]
        public string? Modifier { get; set; }

        public int? StatusId { get; set; }

        public long? ContentId { get; set; }

        public int? FunctionId { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
