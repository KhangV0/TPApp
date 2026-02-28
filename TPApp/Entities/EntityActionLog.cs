using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("EntityActionLogs")]
    public class EntityActionLog
    {
        [Key]
        public long Id { get; set; }

        public int EntityId { get; set; }

        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        public string? MetadataJson { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Well-known action types for EntityActionLog</summary>
    public static class ActionTypes
    {
        public const string View          = "View";
        public const string Rate          = "Rate";
        public const string Download      = "Download";
        public const string Share         = "Share";
        public const string ContactChat   = "ContactChat";
    }
}
