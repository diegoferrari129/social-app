using api.DTOs.ChatDTOs;
using api.Hubs;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly UserService _userService;
        private readonly ILogger<ChatController> _logger;
        private readonly NotificationService _notificationService;
        private readonly IHubContext<NotificationsHub> _notificationsHub;

        public ChatController(ChatService chatService, UserService userService, ILogger<ChatController> logger, NotificationService notificationService, IHubContext<NotificationsHub> notificationsHub)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
            _notificationService = notificationService;
            _notificationsHub = notificationsHub;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId)) return Unauthorized();

            var senderName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            var chatId = await _chatService.GetOrCreateChatAsync(senderId, request.ToUserId);

            var message = await _chatService.SendMessageAsync(chatId, senderId, request.Text);
            if (message == null) return StatusCode(500, new { message = "Failed to send message" });

            var notification = new Notification
            {
                UserId = request.ToUserId,
                Type = "message",
                FromUserId = senderId,
                FromUserName = senderName,
                MessageText = request.Text.Length > 50 ? request.Text.Substring(0, 50) + "..." : request.Text,
                IsRead = false
            };
            await _notificationService.CreateAsync(notification);

            await _notificationsHub.Clients.Group($"notifications-{request.ToUserId}").SendAsync("NewNotification", new
            {
                type = "message",
                fromUserId = senderId,
                fromUserName = senderName,
                chatId = chatId,
                messageText = request.Text.Length > 50 ? request.Text.Substring(0, 50) + "..." : request.Text,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Message sent from {Sender} to {To}", senderId, request.ToUserId);
            return Ok(message);
        }

        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(string conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var conv = (await _chatService.GetUserChatsAsync(userId)).FirstOrDefault(c => c.Id == conversationId);
            if (conv == null)
                return Unauthorized("You are not part of this conversation");

            var messages = await _chatService.GetMessagesAsync(conversationId, userId);
            return Ok(messages);
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetChats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var chats = await _chatService.GetUserChatsAsync(userId);

            var result = new List<ChatResponseDto>();
            foreach (var chat in chats)
            {
                var otherUserId = chat.Participants.First(p => p != userId);
                var otherUser = await _userService.GetByIdAsync(otherUserId);
                var unreadCount = await _chatService.GetUnreadCountForChatAsync(chat.Id, userId);
                result.Add(new ChatResponseDto
                {
                    Id = chat.Id!,
                    OtherUserId = otherUserId,
                    OtherUserName = otherUser?.Name ?? "Unknown",
                    OtherUserImgUrl = otherUser?.ImgUrl ?? "",
                    LastMessage = chat.LastMessage,
                    LastMessageTime = chat.LastMessageTime,
                    UnreadCount = unreadCount
                });
            }
            return Ok(result);
        }

        [HttpPost("messages/mark-read/{conversationId}")]
        [Authorize]
        public async Task<IActionResult> MarkMessagesAsRead(string conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            await _chatService.MarkMessagesAsReadAsync(conversationId, userId);
            return Ok();
        }

        [HttpPost("start/{otherUserId}")]
        public async Task<IActionResult> StartConversation(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var chatId = await _chatService.GetOrCreateChatAsync(userId, otherUserId);
            var chat = await _chatService.GetChatByIdAsync(chatId);
            if (chat == null) return NotFound();

            var otherParticipant = chat.Participants.First(p => p != userId);
            var otherUser = await _userService.GetByIdAsync(otherParticipant);

            return Ok(new ChatResponseDto
            {
                Id = chat.Id!,
                OtherUserId = otherParticipant,
                OtherUserName = otherUser?.Name ?? "Unknown",
                OtherUserImgUrl = otherUser?.ImgUrl ?? "",
                LastMessage = chat.LastMessage,
                LastMessageTime = chat.LastMessageTime
            });
        }
    }
}
