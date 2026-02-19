using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPApp.Enums;

namespace TPApp.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public string? UserId { get; set; }

        /// <summary>Recipient address: email or phone number.</summary>
        [StringLength(500)]
        public string? Target { get; set; }

        [StringLength(500)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        /// <summary>1=Email, 2=SMS</summary>
        public int Channel { get; set; } = (int)NotificationChannel.Email;

        /// <summary>0=Pending, 1=Success, 2=Failed</summary>
        public int Status { get; set; } = (int)NotificationStatus.Pending;

        public int RetryCount { get; set; } = 0;

        [StringLength(1000)]
        public string? LastError { get; set; }

        [StringLength(1000)]
        public string? ProviderResponse { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastTriedDate { get; set; }

        public DateTime? SentDate { get; set; }

        /// <summary>Has the user read/dismissed this notification in the bell UI.</summary>
        public bool IsRead { get; set; } = false;

        public DateTime? ReadDate { get; set; }
    }
}
