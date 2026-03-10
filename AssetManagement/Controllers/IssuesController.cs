using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using AssetManagement.Models.Enums;
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
        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Employee")]
        [HttpGet("my-issues")]
        public async Task<IActionResult> GetMyIssues()
        {
            try
            {
                // Extract UserID from the JWT Token (same logic as your Create method)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                int currentUserId = int.Parse(userIdClaim);

                var issues = await _db.Set<Issue>()
                    .Include(i => i.Asset)
                    .Include(i => i.ReportedBy)
                    // THE KEY FIX: Filter by the logged-in user's ID
                    .Where(i => i.ReportedByUserID == currentUserId)
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
                        i.CreatedAt,
                        i.UpdatedAt,
                        i.CreatedBy,
                        i.UpdatedBy
                    ))
                    .ToListAsync();

                return Ok(issues);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Employee,Manager")] // Allow the person fixing it to update the status
        public async Task<IActionResult> UpdateStatusOnly(int id, [FromBody] IssueStatusUpdateDto dto)
        {
            // 1. Find the Issue
            var issue = await _db.Set<Issue>().FindAsync(id);
            if (issue == null) return NotFound(new { message = "Issue not found." });

            // 2. Update the Issue Status using your Enum
            issue.Status = (IssueStatus)dto.Status;
            issue.UpdatedAt = DateTime.UtcNow;
            issue.UpdatedBy = GetCurrentUserName();

            // 3. The State Machine Logic: If the issue is now Resolved (Assuming 2 = Resolved)
            // Note: Cast dto.Status to your IssueStatus enum to check it
            if ((IssueStatus)dto.Status == IssueStatus.Resolved)
            {
                var asset = await _db.Set<Asset>().FindAsync(issue.AssetID);
                if (asset != null)
                {
                    // Check if there is an active assignment for this asset
                    bool hasActiveAssignment = await _db.Set<Assignment>()
                        .AnyAsync(a => a.AssetID == asset.AssetID && a.Status == AssignmentStatus.Active);

                    // Flip the asset status based on the assignment check
                    asset.Status = hasActiveAssignment ? AssetStatus.Assigned : AssetStatus.Available;

                    asset.UpdatedAt = DateTime.UtcNow;
                    asset.UpdatedBy = GetCurrentUserName();
                }
            }

            // 4. Save all changes (Issue update and Asset update) in one transaction
            await _db.SaveChangesAsync();

            return Ok(new { message = "Issue status updated and Asset routed correctly." });
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