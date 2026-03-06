using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using AssetManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AssetManagement.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class IssuesController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly ISpService _sp;

		public IssuesController(ApplicationDbContext db, ISpService sp)
		{
			_db = db;
			_sp = sp;
		}

		// Helper to extract the logged-in user's name from JWT claims
		private string GetCurrentUserName() => User.Identity?.Name ?? "System";

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var issues = await _db.Set<Issue>()
				.Include(i => i.Asset)
				.Include(i => i.ReportedBy)
				.Where(i => i.Status != AssetManagement.Models.Enums.IssueStatus.Closed)
				.Select(i => new IssueResponseDto(
					i.IssueID,
					i.AssetID,
					i.Asset!.Name,
					i.ReportedByUserID,
					i.ReportedBy.Name,
					i.Description,
					i.ReportedDate,
					i.Status,
					i.CreatedAt, // Include audit fields in your DTO
					i.UpdatedAt,
					i.CreatedBy,
					i.UpdatedBy
				))
				.ToListAsync();

			return Ok(issues);
		}

        [Authorize(Roles = "Employee,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(IssueCreateDto dto)
        {
            try
            {
                // 1. Extract the actual UserID from the NameIdentifier claim
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                int actualUserId = int.Parse(userIdClaim);

                // 2. Call the SP using the ID from the TOKEN, not the DTO
                await _sp.LogIssueAsync(dto.AssetID, actualUserId, dto.Description ?? string.Empty, dto.RequiresRepair);

                return Ok(new { message = "Issue logged successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, IssueUpdateDto dto)
		{
			var issue = await _db.Set<Issue>().FindAsync(id);
			if (issue == null) return NotFound();

			issue.Description = dto.Description;
			issue.Status = dto.Status;

			// --- Manual Audit Fields ---
			issue.UpdatedAt = DateTime.UtcNow;
			issue.UpdatedBy = GetCurrentUserName();

			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var issue = await _db.Set<Issue>().FindAsync(id);
			if (issue == null) return NotFound();

			// Soft-delete/Close logic
			issue.Status = AssetManagement.Models.Enums.IssueStatus.Closed;

			// --- Manual Audit Fields ---
			issue.UpdatedAt = DateTime.UtcNow;
			issue.UpdatedBy = GetCurrentUserName();

			await _db.SaveChangesAsync();
			return NoContent();
		}
	}
}