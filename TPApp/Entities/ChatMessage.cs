using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public long Id { get; set; }

        public long ConversationId { get; set; }

        [MaxLength(450)]
        public string SenderUserId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        /// <summary>Auto-generated system message vs user-typed</summary>
        public bool IsSystem { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ConversationId))]
        public ChatConversation? Conversation { get; set; }
    }
}
