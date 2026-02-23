using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("MucDo")]
    public class MucDo
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }
}
