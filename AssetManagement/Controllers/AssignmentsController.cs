using AssetManagement.DTOs;
using AssetManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AssetManagement.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly ISpService _spService;

        private readonly AssetManagement.Data.ApplicationDbContext _db;

        public AssignmentsController(ISpService spService, AssetManagement.Data.ApplicationDbContext db)
        {
            _spService = spService;
            _db = db;
        }
        [HttpGet]
        public async  Task<IActionResult> GetAll()
        {
            var a = await _db.Assignments
                .Include(a=> a.Asset)
                .Include(a=> a.AssignedToUser)
                .Select(a=> new AssignmentResponseDto(
                    a.AssignmentID,
                    a.AssetID,
                    a.Asset.Name,
                    a.AssignedToUserID,
                    a.AssignedToUser.Name,
                    a.AssignedDate,
                    a.ReturnDate,
                    a.Status,
                    a.Location
                )).ToListAsync();

            return Ok(a);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager")]
        [HttpPost("assign")]
        public async Task<IActionResult> Assign(AssignAssetDto dto)
        {
            await _spService.AssignAssetAsync(dto.AssetID, dto.AssignedToUserID, dto.AssignedDate, dto.Location ?? string.Empty);
            return Ok();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager")]
        [HttpPost("return")]
        public async Task<IActionResult> Return(ReturnAssetDto dto)
        {
            await _spService.ReturnAssetAsync(dto.AssignmentID, dto.ReturnDate);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAssignmentDto dto)
        {
            var a = await _db.Set<AssetManagement.Models.Entities.Assignment>().FindAsync(id);
            if (a == null) return NotFound();

            a.Status = dto.Status;
            a.Location = dto.Location;
            
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
