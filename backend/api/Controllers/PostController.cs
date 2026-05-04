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

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId, page, pageSize);
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(string id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
                return NotFound(new { message = "Post not found" });
            return Ok(post);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] UpdatePostDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existing = await _postService.GetPostByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Post not found" });
            if (existing.UserId != userId)
                return Unauthorized(new { message = "You can only edit your own posts" });

            if (!string.IsNullOrEmpty(request.Title))
                existing.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Content))
                existing.Content = request.Content;
            if (request.PostImg != null)
                existing.PostImg = request.PostImg;

            var updated = await _postService.UpdateAsync(id, existing);
            if (!updated)
                return StatusCode(500, new { message = "Update failed" });

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
                return NotFound(new { message = "Post not found" });
            if (post.UserId != userId)
                return Unauthorized(new { message = "You can only delete your own posts" });

            var deleted = await _postService.DeleteAsync(id);
            if (!deleted)
                return StatusCode(500, new { message = "Deletion failed" });

            return NoContent();
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(string id, [FromBody] CommentDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userId == null || userName == null) return Unauthorized();

            var comment = new Comment
            {
                UserId = userId,
                UserName = userName,
                Text = request.Text
            };

            var success = await _postService.AddCommentAsync(id, comment);
            if (!success) return NotFound();
            return Ok(comment);
        }

        [HttpDelete("{id}/comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(string id, string commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
                return NotFound(new { message = "Post not found" });

            var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            if (comment.UserId != userId)
                return Unauthorized(new { message = "You cannot delete this comment" });

            var success = await _postService.RemoveCommentAsync(id, commentId);
            if (!success)
                return StatusCode(500, new { message = "Deletion failed" });
            return NoContent();
        }
    }
}
