using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int CatId { get; set; }

        [MaxLength(500)]
        public string? Title { get; set; }

        [MaxLength(50)]
        public string? TitleEn { get; set; }

        [MaxLength(500)]
        public string? QueryString { get; set; }

        public int? ParentId { get; set; }

        public int? ParentIdEN { get; set; }

        public int? StatusId { get; set; }

        public double? Viewed { get; set; }

        public int? Sort { get; set; }

        public DateTime? Created { get; set; }

        [MaxLength(50)]
        public string? Creator { get; set; }

        public DateTime? Modified { get; set; }

        [MaxLength(50)]
        public string? Modifier { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? LogoURL { get; set; }

        public bool? MainCate { get; set; }

        [Required]
        public int LanguageId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Domain { get; set; } = string.Empty;

        public int? SiteId { get; set; }

    }
}
