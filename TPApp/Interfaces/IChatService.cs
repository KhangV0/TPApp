namespace TPApp.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// Start or resume a chat conversation between buyer and supplier for a product.
        /// Returns the conversation ID.
        /// </summary>
        Task<ChatStartResult> StartConversationAsync(int productId, int buyerUserId);

        /// <summary>Get all conversations for a user (as buyer or supplier).</summary>
        Task<List<ChatConversationVm>> GetConversationsAsync(int userId);

        /// <summary>Get messages for a conversation (with access check).</summary>
        Task<ChatConversationDetailVm?> GetConversationAsync(long conversationId, int userId);

        /// <summary>Send a message in a conversation.</summary>
        Task<bool> SendMessageAsync(long conversationId, int senderUserId, string message);

        /// <summary>Mark messages as read.</summary>
        Task MarkAsReadAsync(long conversationId, int userId);

        /// <summary>Get unread message count for a user.</summary>
        Task<int> GetUnreadCountAsync(int userId);
    }

    public class ChatStartResult
    {
        public bool Success { get; set; }
        public long ConversationId { get; set; }
        public bool IsNew { get; set; }
        public string? Error { get; set; }
    }

    public class ChatConversationVm
    {
        public long Id { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string OtherUserName { get; set; } = "";
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatConversationDetailVm
    {
        public long ConversationId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string OtherUserName { get; set; } = "";
        public List<ChatMessageVm> Messages { get; set; } = new();
    }

    public class ChatMessageVm
    {
        public long Id { get; set; }
        public string SenderName { get; set; } = "";
        public bool IsMe { get; set; }
        public bool IsSystem { get; set; }
        public string Message { get; set; } = "";
        public DateTime Created { get; set; }
    }
}
