using api.DTOs;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly UserService _userService;
        private readonly ILogger<PostController> _logger;

        public PostController(PostService postService, UserService userService, ILogger<PostController> logger)
        {
            _postService = postService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Title and Content are required" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var post = new Post
            {
                Title = request.Title,
                Content = request.Content,
                PostImg = request.PostImg,
                UserId = userId,
                UserName = user.Name,
                CreatedAt = DateTime.UtcNow,
                Likes = new List<string>(),
                Comments = new List<Comment>()
            };

            await _postService.CreateAsync(post);

            _logger.LogInformation("Post {PostId} created by user {UserId}", post.Id, userId);

            return Ok(post);
        }
    }
}
