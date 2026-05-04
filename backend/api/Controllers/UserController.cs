using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        private readonly IConfiguration _configuration;
        public UserController(UserService userService, ILogger<UserController> logger, IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
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
