using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using AssetManagement.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Helper to extract the logged-in user's name from JWT claims
        private string GetCurrentUserName() => User.Identity?.Name ?? "System";

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // 1. Get the current user's ID from the token
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int currentUserId = int.Parse(currentUserIdClaim ?? "0");

            // 2. Base Query: Start with all non-deleted users
            var query = _db.Users
                .Include(u => u.Role)
                .Where(u => u.Status != UserStatus.Deleted);

            // 3. Apply Filters based on Role
            if (User.IsInRole("Manager"))
            {
                // Managers are strictly locked to viewing only "Employee" roles who have assets.
                query = query.Where(u => u.Role.Name == "Employee" &&
                                        _db.Set<Assignment>().Any(a => a.AssignedToUserID == u.UserID &&
                                                                      a.Status == AssignmentStatus.Active));
            }

            // 4. Execute and Return
            var users = await query
                .Select(a => new UserResponseDto(
                    a.UserID, a.Name, a.Email, a.Role.RoleID, a.Role.Name,
                    a.Department, a.Status, a.CreatedAt, a.UpdatedAt,
                    a.CreatedBy, a.UpdatedBy
                ))
                .ToListAsync();

            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == id && u.Status != UserStatus.Deleted);

            if (user == null) return NotFound();

            // SECURITY: If a Manager tries to direct-access an Admin ID via URL, block it
            if (User.IsInRole("Manager") && user.Role.Name != "Employee")
            {
                return Forbid();
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can modify user details
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

            // Audit Fields
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = GetCurrentUserName();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can delete (soft-delete) users
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Soft delete
            user.Status = UserStatus.Deleted;

            // Audit Fields
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = GetCurrentUserName();

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}