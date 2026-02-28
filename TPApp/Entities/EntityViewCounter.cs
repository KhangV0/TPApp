using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("EntityViewCounters")]
    public class EntityViewCounter
    {
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int EntityId { get; set; }

        public int ViewCount { get; set; }
    }
}
