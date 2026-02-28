using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        private readonly INotificationQueueService _notifQueue;

        public ChatService(AppDbContext context, INotificationQueueService notifQueue)
        {
            _context = context;
            _notifQueue = notifQueue;
        }

        // ── Start or Resume Conversation ────────────────────────────────────────
        public async Task<ChatStartResult> StartConversationAsync(int productId, int buyerUserId)
        {
            // 1) Validate product
            var product = await _context.SanPhamCNTBs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == productId && p.StatusId == 3);

            if (product == null)
                return new ChatStartResult { Success = false, Error = "Sản phẩm không tồn tại." };

            // 2) Determine supplier UserId
            int? supplierUserId = await ResolveSupplierUserIdAsync(product);

            if (supplierUserId == null || supplierUserId == 0)
                return new ChatStartResult { Success = false, Error = "Không tìm thấy nhà cung ứng." };

            // 3) Cannot chat with self
            if (supplierUserId == buyerUserId)
                return new ChatStartResult { Success = false, Error = "Bạn không thể liên hệ chính mình." };

            string buyerIdStr = buyerUserId.ToString();
            string supplierIdStr = supplierUserId.Value.ToString();

            // 4) Check existing conversation
            var existing = await _context.ChatConversations
                .FirstOrDefaultAsync(c =>
                    c.ProductId == productId &&
                    c.BuyerUserId == buyerIdStr &&
                    c.SupplierUserId == supplierIdStr);

            if (existing != null)
            {
                return new ChatStartResult
                {
                    Success = true,
                    ConversationId = existing.Id,
                    IsNew = false
                };
            }

            // 5) Create new conversation + first message
            var conversation = new ChatConversation
            {
                ProductId = productId,
                ProductType = product.ProductType,
                BuyerUserId = buyerIdStr,
                SupplierUserId = supplierIdStr,
                ProductName = product.Name,
                IsFromProductDetail = true,
                Created = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Auto-generated first message with product link
            var productUrl = $"/2-cong-nghe-thiet-bi/{product.ProductType}/{TPApp.Controllers.ProductController.MakeURLFriendly(product.Name)}-{product.ID}.html";
            var firstMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderUserId = buyerIdStr,
                Message = $"Xin chào, tôi quan tâm đến sản phẩm: <a href=\"{productUrl}\" target=\"_blank\">{product.Name}</a>.\nMong được tư vấn thêm.",
                IsSystem = true,
                IsRead = false,
                Created = DateTime.UtcNow
            };
            _context.ChatMessages.Add(firstMessage);
            await _context.SaveChangesAsync();

            // 6) Trigger notification to supplier
            try
            {
                await _notifQueue.QueueAsync(
                    supplierUserId.Value,
                    projectId: null,
                    title: "Có yêu cầu liên hệ mới",
                    content: $"Bạn có tin nhắn mới về sản phẩm: {product.Name}",
                    channel: NotificationChannel.Email,
                    url: $"/chat/{conversation.Id}");
            }
            catch
            {
                // Non-critical — don't fail the chat start
            }

            return new ChatStartResult
            {
                Success = true,
                ConversationId = conversation.Id,
                IsNew = true
            };
        }

        // ── Get Conversations List ──────────────────────────────────────────────
        public async Task<List<ChatConversationVm>> GetConversationsAsync(int userId)
        {
            string uid = userId.ToString();

            var conversations = await _context.ChatConversations
                .AsNoTracking()
                .Where(c => c.BuyerUserId == uid || c.SupplierUserId == uid)
                .OrderByDescending(c => c.LastMessageAt ?? c.Created)
                .ToListAsync();

            var result = new List<ChatConversationVm>();

            foreach (var c in conversations)
            {
                string otherUserId = c.BuyerUserId == uid ? c.SupplierUserId : c.BuyerUserId;

                var lastMsg = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.Created)
                    .Select(m => m.Message)
                    .FirstOrDefaultAsync();

                int unread = await _context.ChatMessages
                    .CountAsync(m =>
                        m.ConversationId == c.Id &&
                        m.SenderUserId != uid &&
                        !m.IsRead);

                // Resolve other user name
                var otherUser = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == int.Parse(otherUserId))
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();

                result.Add(new ChatConversationVm
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    OtherUserName = otherUser ?? "Người dùng",
                    LastMessage = lastMsg,
                    LastMessageAt = c.LastMessageAt,
                    UnreadCount = unread
                });
            }

            return result;
        }

        // ── Get Conversation Detail ─────────────────────────────────────────────
        public async Task<ChatConversationDetailVm?> GetConversationAsync(long conversationId, int userId)
        {
            string uid = userId.ToString();

            var conv = await _context.ChatConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerUserId == uid || c.SupplierUserId == uid));

            if (conv == null) return null;

            string otherUserId = conv.BuyerUserId == uid ? conv.SupplierUserId : conv.BuyerUserId;
            var otherUser = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == int.Parse(otherUserId))
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

            var messages = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.Created)
                .ToListAsync();

            // Batch-resolve sender names
            var senderIds = messages.Select(m => m.SenderUserId).Distinct().ToList();
            var senderNames = new Dictionary<string, string>();
            foreach (var sid in senderIds)
            {
                if (int.TryParse(sid, out int sidInt))
                {
                    var name = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == sidInt)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync();
                    senderNames[sid] = name ?? "Người dùng";
                }
            }

            return new ChatConversationDetailVm
            {
                ConversationId = conv.Id,
                ProductId = conv.ProductId,
                ProductName = conv.ProductName,
                OtherUserName = otherUser ?? "Người dùng",
                Messages = messages.Select(m => new ChatMessageVm
                {
                    Id = m.Id,
                    SenderName = senderNames.GetValueOrDefault(m.SenderUserId, "Người dùng"),
                    IsMe = m.SenderUserId == uid,
                    IsSystem = m.IsSystem,
                    Message = m.Message,
                    Created = m.Created
                }).ToList()
            };
        }

        // ── Send Message ────────────────────────────────────────────────────────
        public async Task<bool> SendMessageAsync(long conversationId, int senderUserId, string message)
        {
            string uid = senderUserId.ToString();

            var conv = await _context.ChatConversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerUserId == uid || c.SupplierUserId == uid));

            if (conv == null) return false;

            var msg = new ChatMessage
            {
                ConversationId = conversationId,
                SenderUserId = uid,
                Message = message,
                IsSystem = false,
                IsRead = false,
                Created = DateTime.UtcNow
            };
            _context.ChatMessages.Add(msg);

            conv.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify the other party
            string recipientId = conv.BuyerUserId == uid ? conv.SupplierUserId : conv.BuyerUserId;
            try
            {
                if (int.TryParse(recipientId, out int recipientInt))
                {
                    await _notifQueue.QueueAsync(
                        recipientInt,
                        projectId: null,
                        title: "Tin nhắn mới",
                        content: $"Bạn có tin nhắn mới về sản phẩm: {conv.ProductName}",
                        channel: NotificationChannel.Email,
                        url: $"/chat/{conversationId}");
                }
            }
            catch { /* non-critical */ }

            return true;
        }

        // ── Mark As Read ────────────────────────────────────────────────────────
        public async Task MarkAsReadAsync(long conversationId, int userId)
        {
            string uid = userId.ToString();

            var unread = await _context.ChatMessages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderUserId != uid &&
                    !m.IsRead)
                .ToListAsync();

            foreach (var m in unread) m.IsRead = true;
            await _context.SaveChangesAsync();
        }

        // ── Unread Count ────────────────────────────────────────────────────────
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            string uid = userId.ToString();

            // Get all conversation IDs where this user is a participant
            var convIds = await _context.ChatConversations
                .AsNoTracking()
                .Where(c => c.BuyerUserId == uid || c.SupplierUserId == uid)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.ChatMessages
                .CountAsync(m =>
                    convIds.Contains(m.ConversationId) &&
                    m.SenderUserId != uid &&
                    !m.IsRead);
        }

        // ── Resolve Supplier UserId from product ────────────────────────────────
        private async Task<int?> ResolveSupplierUserIdAsync(SanPhamCNTB product)
        {
            // Path 1: NCUId → NhaCungUng.UserId
            if (product.NCUId.HasValue && product.NCUId > 0)
            {
                var ncuUserId = await _context.NhaCungUngs
                    .AsNoTracking()
                    .Where(n => n.CungUngId == product.NCUId)
                    .Select(n => n.UserId)
                    .FirstOrDefaultAsync();

                if (ncuUserId.HasValue && ncuUserId > 0)
                    return ncuUserId;
            }

            // Path 2: Creator username → resolve to UserId
            if (!string.IsNullOrEmpty(product.Creator))
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == product.Creator);

                if (user != null)
                    return user.Id;
            }

            // Path 3: StoreId → Store.UserId (if Store entity has UserId)
            if (product.StoreId.HasValue && product.StoreId > 0)
            {
                var storeUserId = await _context.Stores
                    .AsNoTracking()
                    .Where(s => s.StoreId == product.StoreId)
                    .Select(s => s.UserId)
                    .FirstOrDefaultAsync();

                if (storeUserId > 0)
                    return storeUserId;
            }

            return null;
        }
    }
}
