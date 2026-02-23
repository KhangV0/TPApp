using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Status")]
    public class Status
    {
        [Key]
        public int StatusId { get; set; }
        public string? Title { get; set; }
        public string? Domain { get; set; }
        public int LanguageId { get; set; }
        public int ParentId { get; set; }
        public int SiteId { get; set; }
    }
}
