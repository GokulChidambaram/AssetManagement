using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AssetsController(ApplicationDbContext db) => _db = db;

        private string GetCurrentUserName() => User.Identity?.Name ?? "System";

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _db.Assets
                .Include(a => a.Category)
                .Where(a => a.Status != AssetManagement.Models.Enums.AssetStatus.Deleted)
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
                    a.UpdatedAt,
                    a.CreatedBy,
                    a.UpdatedBy
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
                    a.UpdatedAt,
                    a.CreatedBy,
                    a.UpdatedBy
                )).FirstOrDefaultAsync();

            return asset == null ? NotFound() : Ok(asset);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AssetCreateDto dto)
        {
            var currentUser = GetCurrentUserName();

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
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser
            };

            _db.Assets.Add(asset);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = asset.AssetID }, asset);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] AssetUpdateDto dto)
        {
            var asset = await _db.Assets.FindAsync(id);
            if (asset == null) return NotFound();

            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Name == dto.CategoryName);
            if (category == null)
                return BadRequest(new { message = $"Category '{dto.CategoryName}' does not exist." });

            asset.Name = dto.Name;
            asset.ModelNo = dto.ModelNo;
            asset.CategoryID = category.CategoryID;
            asset.Tag = dto.Tag;
            asset.PurchaseDate = dto.PurchaseDate;
            asset.Cost = dto.Cost;
            asset.Status = dto.Status;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = GetCurrentUserName();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var asset = await _db.Assets.FindAsync(id);
            if (asset == null) return NotFound();

            asset.Status = AssetManagement.Models.Enums.AssetStatus.Deleted;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = GetCurrentUserName();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(DateTime startDate, DateTime endDate)
        {
            var finalEnd = endDate.Date.AddDays(1).AddTicks(-1);

            var report = await _db.Assets
                .Where(a => a.CreatedAt >= startDate.Date && a.CreatedAt <= finalEnd)
                .ToListAsync();

            return Ok(report);
        }
    }
}