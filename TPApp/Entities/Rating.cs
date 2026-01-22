using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Rating")]
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        public int? SanPhamId { get; set; }

        public int? UserId { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Contents { get; set; }

        [MaxLength(60)]
        public string? Email { get; set; }

        public DateTime? Created { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        public int? StatusId { get; set; }

        [MaxLength(500)]
        public string? UrlRefer { get; set; }

        public int? RatingValue { get; set; }

        public int? TypeID { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
