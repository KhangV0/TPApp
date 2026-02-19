using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ContractAuditLogs")]
    public class ContractAuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string EntityId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = null!;

        public string? DataJson { get; set; }

        public int? ActorUserId { get; set; }

        [StringLength(100)]
        public string? IPAddress { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
