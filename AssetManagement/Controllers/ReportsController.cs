using AssetManagement.Data;
using AssetManagement.DTOs;
using AssetManagement.Models.Entities;
using AssetManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ISpService _sp;

        public ReportsController(ApplicationDbContext db, ISpService sp)
        {
            _db = db;
            _sp = sp;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Set<Report>().ToListAsync());

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Generate(ReportCreateDto dto)
        {
            await _sp.GenerateReportAsync(dto.Scope ?? string.Empty);
            return Ok();
        }

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var r = await _db.Set<Report>().FindAsync(id);
        if (r == null) return NotFound();
        return Ok(r);
    }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.Set<Report>().FindAsync(id);
            if (r == null) return NotFound();

            r.UpdatedAt = DateTime.UtcNow; // soft-delete via UpdatedAt
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
