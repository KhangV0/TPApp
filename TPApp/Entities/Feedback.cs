using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Feedback")]
    public class Feedback
    {
        [Key]
        public int ID { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public string? Creator { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        public string? Modifier { get; set; }

        public int? StatusId { get; set; }

        public DateTime? PublishedDate { get; set; }

        public DateTime? bEffectiveDate { get; set; }

        public DateTime? eEffectiveDate { get; set; }

        public string? Domain { get; set; }

        public int? LanguageId { get; set; }

        public int? DeptId { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
