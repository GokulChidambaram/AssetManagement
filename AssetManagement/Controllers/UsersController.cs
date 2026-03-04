using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManagement.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
	public class UsersController : ControllerBase
	{
		private readonly ApplicationDbContext _db;

		public UsersController(ApplicationDbContext db)
		{
			_db = db;
		}

		// Helper to extract the logged-in admin's name from JWT claims
		private string GetCurrentUserName() => User.Identity?.Name ?? "System";

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var users = await _db.Users
				.Include(a => a.Role)
				.Where(a => a.Status != AssetManagement.Models.Enums.UserStatus.Deleted)
				.Select(a => new UserResponseDto(
					a.UserID,
					a.Name,
					a.Email,
					a.Role.RoleID,
					a.Role.Name,
					a.Department,
					a.Status,
					a.CreatedAt,   // Added to DTO
					a.UpdatedAt,   // Added to DTO
					a.CreatedBy,   // Added to DTO
					a.UpdatedBy    // Added to DTO
					))
				.ToListAsync();

			return Ok(users);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			// Using FirstOrDefaultAsync to include Role if needed for the response
			var user = await _db.Users
				.Include(u => u.Role)
				.FirstOrDefaultAsync(u => u.UserID == id && u.Status != AssetManagement.Models.Enums.UserStatus.Deleted);

			if (user == null) return NotFound();
			return Ok(user);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, UserUpdateDto dto)
		{
			var user = await _db.Users.FindAsync(id);
			if (user == null) return NotFound();

			// Update user properties
			user.Name = dto.Name;
			user.Email = dto.Email;
			user.RoleID = dto.RoleID;
			user.Department = dto.Department;
			user.Status = dto.Status;

			// Audit Fields: Record who is performing the update
			user.UpdatedAt = DateTime.UtcNow;
			user.UpdatedBy = GetCurrentUserName();

			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var user = await _db.Users.FindAsync(id);
			if (user == null) return NotFound();

			// Soft delete the user
			user.Status = AssetManagement.Models.Enums.UserStatus.Deleted;

			// Audit Fields: Record who performed the deletion
			user.UpdatedAt = DateTime.UtcNow;
			user.UpdatedBy = GetCurrentUserName();

			await _db.SaveChangesAsync();
			return NoContent();
		}
	}
}