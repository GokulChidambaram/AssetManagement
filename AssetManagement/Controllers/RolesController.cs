using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Uncommented for security
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RolesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Helper to extract the logged-in admin's name from JWT claims
        private string GetCurrentUserName() => User.Identity?.Name ?? "System";

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _db.Roles
               .Where(r => !r.IsDeleted)
               .ToListAsync();
            return Ok(roles);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.RoleID == id && !r.IsDeleted);

            if (role == null) return NotFound(new { message = $"Role with ID {id} not found." });

            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleCreateDto dto)
        {
            var currentUser = GetCurrentUserName();

            var role = new Role
            {
                Name = dto.Name,

                // --- Manual Audit Fields ---
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser
            };

            _db.Set<Role>().Add(role);
            await _db.SaveChangesAsync();

            // Note: CreatedAtAction usually points to a 'GetById' method if you have one
            return CreatedAtAction(nameof(GetAll), new { id = role.RoleID }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoleUpdateDto dto)
        {
            var role = await _db.Set<Role>().FindAsync(id);
            if (role == null) return NotFound();

            role.Name = dto.Name;

            // --- Manual Audit Fields ---
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = GetCurrentUserName();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _db.Set<Role>().FindAsync(id);
            if (role == null) return NotFound();

            // Soft-delete logic
            role.IsDeleted = true;

            // --- Manual Audit Fields ---
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = GetCurrentUserName();

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}