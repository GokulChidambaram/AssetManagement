using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using AssetManagement.Models.Enums;
using AssetManagement.Services;
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
        private readonly ISpService _spService; // 👈 Inject the SP Service

        public AssetRequestsController(ApplicationDbContext db, ISpService spService)
        {
            _db = db;
            _spService = spService;
        }

        private int GetCurrentUserID() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string GetCurrentUserName() => User.Identity?.Name ?? "System";

        // 1. FOR EMPLOYEES: See their own requests
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetCurrentUserID();

            var requests = await _db.AssetRequests
                .Include(r => r.Category)
                .Where(r => r.RequestedByUserID == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AssetRequestResponseDto(
                    r.RequestID,
                    r.Category!.Name,
                    r.Reason,
                    r.Status.ToString(),
                    r.CreatedAt
                ))
                .ToListAsync();

            return Ok(requests);
        }

        // 2. FOR MANAGERS: See ALL pending requests to approve
        [HttpGet("pending")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetAllPending()
        {
            var requests = await _db.AssetRequests
                .Include(r => r.Category)
                .Include(r => r.RequestedBy) // To show "Santosh" in the table
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new {
                    r.RequestID,
                    EmployeeName = r.RequestedBy!.Name,
                    r.CategoryID,
                    CategoryName = r.Category!.Name,
                    r.Reason,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        // 3. EMPLOYEE: Submit new request
        [HttpPost]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> Create(AssetRequestCreateDto dto)
        {
            var request = new AssetRequest
            {
                CategoryID = dto.CategoryID,
                RequestedByUserID = GetCurrentUserID(),
                Reason = dto.Reason,
                Status = RequestStatus.Pending,
                CreatedBy = GetCurrentUserName(),
                CreatedAt = DateTime.UtcNow
            };

            _db.AssetRequests.Add(request);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Request submitted successfully!" });
        }

        // 4. MANAGER: The Approval Action
        // Logic: Approves the request and triggers the Stored Procedure Assignment
        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Approve(int id, [FromBody] int assetId)
        {
            var request = await _db.AssetRequests.FindAsync(id);
            if (request == null) return NotFound("Request not found");

            if (request.Status != RequestStatus.Pending)
                return BadRequest("This request is already processed.");

            try
            {
                // 🔥 Step 1: Trigger the existing Stored Procedure logic
                // This marks the asset as 'Assigned' and creates the t_assignments record
                await _spService.AssignAssetAsync(
                    assetId,
                    request.RequestedByUserID,
                    DateTime.UtcNow,
                    "Office" // Default location
                );

                // 🔥 Step 2: Update the Request Status to Approved
                request.Status = RequestStatus.Approved;
                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = GetCurrentUserName();

                await _db.SaveChangesAsync();

                return Ok(new { message = "Request approved and asset assigned!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Assignment Failed: {ex.Message}" });
            }
        }
    }
}