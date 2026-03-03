using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AssetManage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {

        private readonly ApplicationDbContext _db;
        public AssetsController(ApplicationDbContext db)
        {
            _db = db;
        }
		private string GetCurrentUserName() => User.Identity?.Name ?? "System";
		//[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Manager,Employee")]
		[HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _db.Assets
                .Include(a => a.Category).Where(a=> a.Status != AssetManagement.Models.Enums.AssetStatus.Deleted)
                .Select(a => new AssetResponseDto(
                    a.AssetID,
                    a.Name,
                    a.CategoryID,
                    a.Category!.Name,
                    a.Tag,
                    a.PurchaseDate,
                    a.ModelNo,
                    a.Cost,
                    a.Status,
                    a.CreatedAt,
                    a.UpdatedAt

                )).ToListAsync();
            return Ok(assets);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> Get(int id)
        {
            
            var asset = await _db.Assets
                .Include(a => a.Category)
                .Where(a => a.AssetID == id && a.Status != AssetManagement.Models.Enums.AssetStatus.Deleted)
                .Select(a => new AssetResponseDto(
                    a.AssetID,
                    a.Name,
                    a.CategoryID,
                    a.Category!.Name,
                    a.Tag,
                    a.PurchaseDate,
                    a.ModelNo,
                    a.Cost,
                    a.Status,
                    a.CreatedAt,
                    a.UpdatedAt
                )).FirstOrDefaultAsync();

            if (asset == null)
                return NotFound();

            return Ok(asset);

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AssetCreateDto dto)

        {
            var asset = new Asset
            {

                Name = dto.Name,
                CategoryID = dto.CategoryID,
                Description = dto.Description,
                ModelNo = dto.ModelNo,
                DepartmentName = dto.DepartmentName,
                SupplierName = dto.SupplierName,
                Tag = dto.Tag,
                PurchaseDate = dto.PurchaseDate,
                Cost = dto.Cost,
                Status = AssetManagement.Models.Enums.AssetStatus.Available,
                CreatedAt = DateTime.UtcNow,

				
			};

            _db.Set<Asset>().Add(asset);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = asset.AssetID }, asset);

        }

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Update(int id, [FromBody] AssetUpdateDto dto) // Added [FromBody]
		{
			var asset = await _db.Assets.FindAsync(id);
			if (asset == null) return NotFound();

			// Look up Category by Name
			var category = await _db.Categories
				.FirstOrDefaultAsync(c => c.Name == dto.CategoryName);

			if (category == null)
			{
				return BadRequest(new { message = $"Category '{dto.CategoryName}' does not exist." });
			}

			// Update properties
			asset.Name = dto.Name;
			asset.ModelNo = dto.ModelNo;
			asset.CategoryID = category.CategoryID;
			asset.Tag = dto.Tag;
			asset.PurchaseDate = dto.PurchaseDate;
			asset.Cost = dto.Cost;
			asset.Status = dto.Status;
			asset.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {

            var asset = await _db.Set<Asset>().FindAsync(id);
            if (asset == null) return NotFound();
            asset.Status = AssetManagement.Models.Enums.AssetStatus.Deleted;
            asset.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();

        }

    }

}

