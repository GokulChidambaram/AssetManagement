using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using AssetManagement.Models.Enums;
using AssetManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ISpService _spService;

        public AssignmentsController(ApplicationDbContext db, ISpService spService)
        {
            _db = db;
            _spService = spService;
        }

        // Helper to extract Name from JWT Claims
        private string GetCurrentUserName() => User.Identity?.Name ?? "System";

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var assignments = await _db.Set<Assignment>()
                .Include(a => a.Asset)
                .Include(a => a.AssignedToUser)
                .Select(a => new AssignmentResponseDto(
                    a.AssignmentID,
                    a.AssetID,
                    a.Asset!.Name,
                    a.AssignedToUserID,
                    a.AssignedToUser!.Name,
                    a.AssignedDate,
                    a.ReturnDate,
                    a.Status,
                    a.Location,
                    a.CreatedAt,
                    a.UpdatedAt,
                    a.CreatedBy,
                    a.UpdatedBy
                )).ToListAsync();

            return Ok(assignments);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetById(int id)
        {
            var a = await _db.Set<Assignment>()
                .Include(a => a.Asset)
                .Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(x => x.AssignmentID == id);

            if (a == null) return NotFound();

            return Ok(new AssignmentResponseDto(
                a.AssignmentID, a.AssetID, a.Asset!.Name,
                a.AssignedToUserID, a.AssignedToUser!.Name,
                a.AssignedDate, a.ReturnDate, a.Status, a.Location,
                a.CreatedAt, a.UpdatedAt, a.CreatedBy, a.UpdatedBy
            ));
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Assign(AssignAssetDto dto)
        {
            try
            {
                // 🔥 Calls your sp_AssignAsset through the service
                // Note: Ensure your ISpService implementation passes GetCurrentUserName() 
                // to the SP if you updated it to include CreatedBy.
                await _spService.AssignAssetAsync(
                    dto.AssetID,
                    dto.AssignedToUserID,
                    dto.AssignedDate,
                    dto.Location ?? "Office"
                );

                return Ok(new { message = "Asset assigned successfully via Stored Procedure." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("return")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Return(ReturnAssetDto dto)
        {
            try
            {
                // 🔥 Calls your sp_ReturnAsset through the service
                await _spService.ReturnAssetAsync(dto.AssignmentID, dto.ReturnDate);
                return Ok(new { message = "Asset returned successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, UpdateAssignmentDto dto)
        {
            var a = await _db.Set<Assignment>().FindAsync(id);
            if (a == null) return NotFound();

            // Only update fields allowed for manual editing
            a.Status = dto.Status;
            a.Location = dto.Location;

            // Manual Audit Fields
            a.UpdatedAt = DateTime.UtcNow;
            a.UpdatedBy = GetCurrentUserName();

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}