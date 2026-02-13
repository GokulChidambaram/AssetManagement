using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<AssetManagement.Models.Entities.User> _passwordHasher;
        private readonly IConfiguration? _config;

        public AuthController(ApplicationDbContext db, Microsoft.AspNetCore.Identity.IPasswordHasher<AssetManagement.Models.Entities.User> passwordHasher, IConfiguration? config = null)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var existing = await _db.Set<User>().FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
            {
                return Conflict(new { message = "Email already registered" });
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                RoleID = dto.RoleID,
                Department = dto.Department,
                Status = AssetManagement.Models.Enums.UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            _db.Set<User>().Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction("Get", "Users", new { id = user.UserID }, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized();
            if (user.Status != AssetManagement.Models.Enums.UserStatus.Active) return Unauthorized();
            var res = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, dto.Password);
            if (res == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed) return Unauthorized();

            // Build JWT token with Role name
            var role = await _db.Set<AssetManagement.Models.Entities.Role>().FindAsync(user.RoleID);
            var roleName = role?.Name ?? "Employee";

            var key = _config?["Jwt:Key"] ?? "please-change-this-secret";
            var issuer = _config?["Jwt:Issuer"] ?? "AssetManage";
            var audience = _config?["Jwt:Audience"] ?? "AssetManageUsers";

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Name),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleName)
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(issuer,
              audience,
              claims,
              expires: DateTime.UtcNow.AddHours(8),
              signingCredentials: credentials);

            var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString, user = new { user.UserID, user.Name, user.Email, role = roleName, user.Department, user.Status } });
        }
    }
}
