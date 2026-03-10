using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AssetManagement.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IPasswordHasher<User> _passwordHasher;
		private readonly IConfiguration? _config;

		public AuthController(ApplicationDbContext db, IPasswordHasher<User> passwordHasher, IConfiguration? config = null)
		{
			_db = db;
			_passwordHasher = passwordHasher;
			_config = config;
		}
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost("register")]
		public async Task<IActionResult> Register(UserRegisterDto dto)
		{
			// 1. Check if user already exists
			var existing = await _db.Set<User>().FirstOrDefaultAsync(u => u.Email == dto.Email);
			if (existing != null)
			{
				return Conflict(new { message = "Email already registered" });
			}

			// 2. Determine who is creating this user
			// If an Admin is logged in, use their name. Otherwise, it's a "Self-Registration".
			var currentUserName = User.Identity?.IsAuthenticated == true
								  ? User.Identity.Name
								  : "Self-Registration";

			var user = new User
			{
				Name = dto.Name,
				Email = dto.Email,
				RoleID = dto.RoleID,
				Department = dto.Department,
				Status = AssetManagement.Models.Enums.UserStatus.Active,

				// --- Manual Audit Fields ---
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				CreatedBy = currentUserName,
				UpdatedBy = currentUserName
			};

			// 3. Hash password and Save
			user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

			_db.Set<User>().Add(user);
			await _db.SaveChangesAsync();

			// Note: CreatedAtAction points to the UsersController "Get" method
			return CreatedAtAction("Get", "Users", new { id = user.UserID }, user);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(UserLoginDto dto)
		{
			// 1. Find User
			var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Email == dto.Email);
			if (user == null) return Unauthorized(new { message = "Invalid email or password" });

			// 2. Check Status
			if (user.Status != AssetManagement.Models.Enums.UserStatus.Active)
				return Unauthorized(new { message = "User account is not active" });

			// 3. Verify Password
			var res = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, dto.Password);
			if (res == PasswordVerificationResult.Failed)
				return Unauthorized(new { message = "Invalid email or password" });

			// 4. Get Role Name for the Claim
			var role = await _db.Set<Role>().FindAsync(user.RoleID);
			var roleName = role?.Name ?? "Employee";

			// 5. Generate JWT Token
			var key = _config?["Jwt:Key"] ?? "please-change-this-secret-key-to-32-chars";
			var issuer = _config?["Jwt:Issuer"] ?? "AssetManage";
			var audience = _config?["Jwt:Audience"] ?? "AssetManageUsers";

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
				new Claim(ClaimTypes.Name, user.Name), // This is what we use for CreatedBy/UpdatedBy
                new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, roleName)
			};

			var token = new JwtSecurityToken(
				issuer,
				audience,
				claims,
				expires: DateTime.UtcNow.AddHours(8),
				signingCredentials: credentials);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

			// 6. Return Response
			return Ok(new
			{
				token = tokenString,
				user = new
				{
					user.UserID,
					user.Name,
					user.Email,
					role = roleName,
					user.Department,
					user.Status
				}
			});
		}
	}
}