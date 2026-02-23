using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Log")]
    public class Log
    {
        [Key]
        public int LogID { get; set; }
        public int? FunctionID { get; set; }
        public DateTime? ActTime { get; set; }
        public int? EventID { get; set; }
        public string? Content { get; set; }
        public string? ClientIP { get; set; }
        public string? UserName { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }
}
