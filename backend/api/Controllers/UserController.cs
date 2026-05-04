
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

using api.DTOs;
using api.Models;
using api.Services;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
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

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Bio,
                user.ImgUrl
            });
        }
    }
}
