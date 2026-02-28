using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("EntityRatings")]
    public class EntityRating
    {
        [Key]
        public long Id { get; set; }

        public int EntityId { get; set; }

        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int Stars { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Comment { get; set; }

        /// <summary>1=Active, 0=Hidden/Moderated</summary>
        public int StatusId { get; set; } = 1;

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Modified { get; set; }
    }
}
