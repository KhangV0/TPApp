using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Domain.Entities
{
    /// <summary>
    /// Represents a logged AI search query for analytics and monitoring.
    /// </summary>
    [Table("AISearchLogs")]
    public class AISearchLog
    {
        /// <summary>
        /// Unique identifier for the log entry
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The search query text entered by the user
        /// </summary>
        [Required]
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string QueryText { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the search was performed
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Number of suppliers returned in the search results
        /// </summary>
        [Required]
        public int ResultCount { get; set; }
    }
}
