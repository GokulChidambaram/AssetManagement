using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RolesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _db.Roles
               .Where(r => !r.IsDeleted)
               .ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleCreateDto dto)
        {
            var role = new Role { Name = dto.Name, CreatedAt = DateTime.UtcNow };
            _db.Set<Role>().Add(role);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = role.RoleID }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoleUpdateDto dto)
        {
            var role = await _db.Set<Role>().FindAsync(id);
            if (role == null) return NotFound();

            role.Name = dto.Name;
            role.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _db.Set<Role>().FindAsync(id);
            if (role == null) return NotFound();

            // soft-delete by setting UpdatedAt (roles may not have a Status in model)

            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
