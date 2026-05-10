using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;
        private readonly UserService _userService;

        public ChatHub(ChatService chatService, UserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string toUserId, string messageText)
        {
            var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(fromUserId)) return;

            var fromUser = await _userService.GetByIdAsync(fromUserId);
            var toUser = await _userService.GetByIdAsync(toUserId);
            if (fromUser == null || toUser == null) return;

            var chatId = await _chatService.GetOrCreateChatAsync(fromUserId, toUserId);

            var message = new Message
            {
                ChatId = chatId,
                SenderId = fromUserId,
                Text = messageText,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            await _chatService.SendMessageAsync(chatId, fromUserId, messageText);

            await Clients.Group($"user-{toUserId}").SendAsync("ReceiveMessage", chatId, fromUserId, fromUser.Name, messageText, DateTime.UtcNow);
            await Clients.Group($"user-{fromUserId}").SendAsync("ReceiveMessage", chatId, fromUserId, fromUser.Name, messageText, DateTime.UtcNow);
        }
    }
}