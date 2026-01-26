using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ImagesAdver")]
    public class ImageAdver
    {
        public int ID { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? SRC { get; set; }

        public string? URL { get; set; }

        public int? Subject { get; set; }

        public int? StatusID { get; set; }

        public DateTime? Created { get; set; }

        public string? Creator { get; set; }

        public DateTime? Modified { get; set; }

        public string? Modifier { get; set; }

        public int? Sort { get; set; }

        public int LanguageID { get; set; }

        public string Domain { get; set; } = null!;

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
