using api.DTOs;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly NotificationService _notificationService;
        public UserController(UserService userService, ILogger<UserController> logger, IConfiguration configuration, NotificationService notificationService)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        {

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "email and password required" });

            var user = new User
            {
                Name = $"{request.FirstName} {request.LastName}".Trim(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Bio = "",
                ImgUrl = ""
            };

            await _userService.CreateAsync(user);

            _logger.LogInformation("user created: {Email} with ID {Id}", user.Email, user.Id);

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Bio,
                    user.ImgUrl
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            bool passwordIsValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!passwordIsValid)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Bio,
                    user.ImgUrl
                }
            });
        }

        [HttpPatch("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != id)
                return Unauthorized(new { message = "You can only update your own profile" });

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Bio))
                user.Bio = request.Bio;
            if (!string.IsNullOrWhiteSpace(request.ImgUrl))
                user.ImgUrl = request.ImgUrl;

            await _userService.UpdateAsync(id, user);

            _logger.LogInformation("User {Id} updated", id);

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Bio,
                user.ImgUrl
            });
        }

        [HttpPost("follow/{targetId}")]
        [Authorize]
        public async Task<IActionResult> FollowUser(string targetId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { message = "User not authenticated" });

            if (currentUserId == targetId)
                return BadRequest(new { message = "You cannot follow yourself" });

            var currentUser = await _userService.GetByIdAsync(currentUserId);
            var targetUser = await _userService.GetByIdAsync(targetId);

            if (currentUser == null || targetUser == null)
                return NotFound(new { message = "User not found" });

            bool isFollowing = currentUser.Following.Contains(targetId);

            if (isFollowing)
            {
                currentUser.Following.Remove(targetId);
                targetUser.Followers.Remove(currentUserId);
                _logger.LogInformation("User {CurrentId} unfollowed {TargetId}", currentUserId, targetId);
            }
            else
            {
                currentUser.Following.Add(targetId);
                targetUser.Followers.Add(currentUserId);
                _logger.LogInformation("User {CurrentId} started following {TargetId}", currentUserId, targetId);
            }

            if (!isFollowing)
            {
                var notification = new Notification
                {
                    UserId = targetId,
                    Type = "follow",
                    FromUserId = currentUserId,
                    FromUserName = currentUser.Name,
                    IsRead = false
                };

                await _notificationService.CreateAsync(notification);
            }

            await _userService.UpdateAsync(currentUserId, currentUser);
            await _userService.UpdateAsync(targetId, targetUser);

            return Ok(new
            {
                success = true,
                action = isFollowing ? "unfollowed" : "followed",
                targetUserId = targetId
            });
        }

        [HttpGet("suggested")]
        [Authorize]
        public async Task<IActionResult> GetSuggestedUsers([FromQuery] int limit = 10)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { message = "Utente non autenticato" });

            var suggestedUsers = await _userService.GetSuggestedUsersAsync(currentUserId, limit);

            var result = suggestedUsers.Select(u => new
            {
                u.Id,
                u.Name,
                u.Bio,
                u.ImgUrl,
                FollowersCount = u.Followers?.Count ?? 0
            });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null || currentUserId != id)
                return Unauthorized(new { message = "You can only delete your own account" });

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var deleted = await _userService.DeleteAsync(id);
            if (!deleted)
                return StatusCode(500, new { message = "Deletion failed" });

            _logger.LogInformation("User {UserId} deleted their account", id);

            return NoContent();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["JwtSecrets:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? throw new InvalidOperationException("User Id is null")),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? throw new InvalidOperationException("User Id is null")),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Name ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: "https://localhost:7022",
                audience: "https://localhost:7022",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
