using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Set<Category>()
                .Where(x=> !x.IsDeleted)
                .ToListAsync();
            return Ok(list);
        }
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			// Find the category by ID and ensure it isn't soft-deleted
			var category = await _db.Set<Category>().FirstOrDefaultAsync(x => x.CategoryID == id && !x.IsDeleted);

			if (category == null)
			{
				return NotFound("Category not found in database.");
			}

			return Ok(category);
		}

		//[HttpPost]
		//public async Task<IActionResult> Create(CategoryCreateDto dto)
		//{
		//    var c = new Category { Name = dto.Name, CreatedAt = DateTime.UtcNow };
		//    _db.Set<Category>().Add(c);
		//    await _db.SaveChangesAsync();
		//    return CreatedAtAction(nameof(GetAll), new { id = c.CategoryID }, c);
		//}
		[HttpPost]
		public async Task<IActionResult> Create(CategoryCreateDto dto)
		{
			try
			{
				var exists = await _db.Set<Category>().AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower() && !x.IsDeleted);
				if (exists) return BadRequest("This category already exists.");

				var category = new Category
				{
					Name = dto.Name,
					CreatedAt = DateTime.UtcNow,
					IsDeleted = false,
					// FIX: You MUST provide these because your Model requires them
					CreatedBy = "Admin",
					UpdatedBy = "Admin",
					UpdatedAt = DateTime.UtcNow
				};

				_db.Set<Category>().Add(category);
				await _db.SaveChangesAsync();

				return Ok(category);
			}
			catch (Exception ex)
			{
				// This will now show the REAL error instead of [object Object]
				return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
			}
		}

		[HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
        {
            var c = await _db.Set<Category>().FindAsync(id);
            if (c == null) return NotFound();

            c.Name = dto.Name;
            c.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Set<Category>().FindAsync(id);
            if (c == null) return NotFound();
            c.IsDeleted = true;
            c.UpdatedAt = DateTime.UtcNow; // soft-delete via UpdatedAt/status not present
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
