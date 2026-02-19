using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("SYS_PARAMETERS")]
    public class SystemParameter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Val { get; set; } = "";

        [StringLength(50)]
        public string? Val2 { get; set; }

        public int? Type { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public bool? IsSystem { get; set; }

        public bool Activated { get; set; }

        [Required]
        [StringLength(500)]
        public string Domain { get; set; } = "";

        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }
}
