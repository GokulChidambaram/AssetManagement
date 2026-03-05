using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AssetRequestsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Helper to extract UserID and Name from the JWT Token
        private int GetCurrentUserID() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string GetCurrentUserName() => User.Identity?.Name ?? "System";

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetCurrentUserID();

            // Fetch requests for the logged-in user only
            var requests = await _db.AssetRequests
                .Include(r => r.Category)
                .Where(r => r.RequestedByUserID == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AssetRequestResponseDto(
                    r.RequestID,
                    r.Category!.Name, // Get the name like "Laptop" instead of ID
                    r.Reason,
                    r.Status.ToString(), // Convert Enum to String for the frontend
                    r.CreatedAt
                ))
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> Create(AssetRequestCreateDto dto)
        {
            var request = new AssetRequest
            {
                CategoryID = dto.CategoryID,
                RequestedByUserID = GetCurrentUserID(),
                Reason = dto.Reason,
                Status = AssetManagement.Models.Enums.RequestStatus.Pending, // Default to Pending
                CreatedBy = GetCurrentUserName(),
                CreatedAt = DateTime.UtcNow
            };

            _db.AssetRequests.Add(request);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Request submitted successfully!" });
        }
    }
}