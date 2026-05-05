using api.DTOs;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ChatController(ChatService chatService, UserService userService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId)) return Unauthorized();

            var chatId = await _chatService.GetOrCreateChatAsync(senderId, request.ToUserId);
            var message = await _chatService.SendMessageAsync(chatId, senderId, request.Text);

            _logger.LogInformation("Message sent from {Sender} to {To}", senderId, request.ToUserId);
            return Ok(message);
        }
    }
}
