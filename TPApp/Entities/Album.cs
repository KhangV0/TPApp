using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Album")]
    public class Album
    {
        [Key]
        public int AlbumID { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public int ContensID { get; set; }

        [StringLength(500)]
        public string Domain { get; set; } = string.Empty;

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
