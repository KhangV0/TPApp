using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? 0;
        }

        // ── POST /api/chat/start ────────────────────────────────────────────
        [HttpPost("/api/chat/start")]
        public async Task<IActionResult> StartConversation([FromBody] ChatStartRequest request)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Unauthorized(new { success = false, error = "Vui lòng đăng nhập." });

            var result = await _chatService.StartConversationAsync(request.ProductId, userId);

            if (!result.Success)
                return BadRequest(new { success = false, error = result.Error });

            return Ok(new
            {
                success = true,
                conversationId = result.ConversationId,
                isNew = result.IsNew,
                redirectUrl = $"/chat/{result.ConversationId}"
            });
        }

        // ── GET /chat ───────────────────────────────────────────────────────
        [HttpGet("/chat")]
        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Redirect("/dang-nhap.html");

            var conversations = await _chatService.GetConversationsAsync(userId);
            return View(conversations);
        }

        // ── GET /chat/{id} ──────────────────────────────────────────────────
        [HttpGet("/chat/{id:long}")]
        public async Task<IActionResult> Conversation(long id)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Redirect("/dang-nhap.html");

            // Mark messages as read
            await _chatService.MarkAsReadAsync(id, userId);

            var detail = await _chatService.GetConversationAsync(id, userId);
            if (detail == null) return NotFound();

            return View(detail);
        }

        // ── POST /api/chat/{id}/send ────────────────────────────────────────
        [HttpPost("/api/chat/{id:long}/send")]
        public async Task<IActionResult> SendMessage(long id, [FromBody] ChatSendRequest request)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Unauthorized(new { success = false });

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, error = "Tin nhắn không được để trống." });

            var ok = await _chatService.SendMessageAsync(id, userId, request.Message.Trim());
            if (!ok) return BadRequest(new { success = false, error = "Không tìm thấy cuộc trò chuyện." });

            return Ok(new { success = true });
        }

        // ── GET /api/chat/unread-count ──────────────────────────────────────
        [HttpGet("/api/chat/unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Ok(new { count = 0 });

            var count = await _chatService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        // ── GET /api/chat/{id}/messages?afterId=0 ───────────────────────────
        [HttpGet("/api/chat/{id:long}/messages")]
        public async Task<IActionResult> GetMessages(long id, [FromQuery] long afterId = 0)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Unauthorized();

            var detail = await _chatService.GetConversationAsync(id, userId);
            if (detail == null) return NotFound();

            // Mark as read
            await _chatService.MarkAsReadAsync(id, userId);

            var newMessages = detail.Messages
                .Where(m => m.Id > afterId)
                .Select(m => new
                {
                    m.Id,
                    m.SenderName,
                    m.IsMe,
                    m.IsSystem,
                    m.Message,
                    created = m.Created.ToString("dd/MM HH:mm")
                })
                .ToList();

            return Ok(new { messages = newMessages });
        }
    }

    // ── Request DTOs ────────────────────────────────────────────────────────
    public class ChatStartRequest
    {
        public int ProductId { get; set; }
    }

    public class ChatSendRequest
    {
        public string Message { get; set; } = "";
    }
}
