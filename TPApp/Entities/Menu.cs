using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Menu")]
    public class Menu
    {
        [Key]
        public int MenuId { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public int? Sort { get; set; }

        public string? MenuPosition { get; set; }

        public byte? StatusId { get; set; }

        public DateTime? Created { get; set; }
        public string? Creator { get; set; }

        public DateTime? Modified { get; set; }
        public string? Modifier { get; set; }

        public DateTime? bEffectiveDate { get; set; }
        public DateTime? eEffectiveDate { get; set; }

        public int? ParentId { get; set; }

        public DateTime? PublishedDate { get; set; }

        public string? QueryString { get; set; }
        public string? NavigateUrl { get; set; }

        public byte? ShowRight { get; set; }

        public string? TitlePage { get; set; }

        public int LanguageId { get; set; }

        public string? ImageLogo { get; set; }

        public string Domain { get; set; } = string.Empty;

        public int? SiteId { get; set; }
    }
}
