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
	public class CategoriesController : ControllerBase
	{
		private readonly ApplicationDbContext _db;

		public CategoriesController(ApplicationDbContext db)
		{
			_db = db;
		}

		// Helper to extract Name from Claims
		private string GetCurrentUserName() => User.Identity?.Name ?? "System";

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var list = await _db.Set<Category>()
				.Where(x => !x.IsDeleted)
				.ToListAsync();
			return Ok(list);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var category = await _db.Set<Category>()
				.FirstOrDefaultAsync(x => x.CategoryID == id && !x.IsDeleted);

			if (category == null)
			{
				return NotFound("Category not found in database.");
			}

			return Ok(category);
		}

		[HttpPost]
		public async Task<IActionResult> Create(CategoryCreateDto dto)
		{
			var currentUserName = GetCurrentUserName();

			var c = new Category
			{
				Name = dto.Name,
				CreatedAt = DateTime.UtcNow,
				CreatedBy = currentUserName, // Manual set
				UpdatedBy = currentUserName  // Manual set
			};

			_db.Set<Category>().Add(c);
			await _db.SaveChangesAsync();

			return CreatedAtAction(nameof(GetById), new { id = c.CategoryID }, c);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
		{
			var c = await _db.Set<Category>().FindAsync(id);
			if (c == null) return NotFound();

			c.Name = dto.Name;

			// Update audit fields
			c.UpdatedAt = DateTime.UtcNow;
			c.UpdatedBy = GetCurrentUserName();

			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var c = await _db.Set<Category>().FindAsync(id);
			if (c == null) return NotFound();

			// Soft delete
			c.IsDeleted = true;

			// Record who deleted it
			c.UpdatedAt = DateTime.UtcNow;
			c.UpdatedBy = GetCurrentUserName();

			await _db.SaveChangesAsync();
			return NoContent();
		}
	}
}