using AssetManagement.Data;
using AssetManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context, ApplicationDbContext db)
        {
            var user = context.User;
            var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = user?.Identity?.Name;
            var role = user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var endpoint = context.Request.Path;
            var method = context.Request.Method;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            // Capture changes AFTER request executes
            await _next(context);
            var entries = db.ChangeTracker.Entries()
                .Where(e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted);
            foreach (var entry in entries)
            {
                var audit = new AuditLog
                {
                    UserID = string.IsNullOrEmpty(userId) ? null : int.Parse(userId),
                    UserName = userName,
                    Role = role,
                    Action = entry.State.ToString().ToUpper(),
                    EntityName = entry.Entity.GetType().Name,
                    EntityID = entry.Properties
                                    .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?
                                    .CurrentValue?.ToString(),
                    HttpMethod = method,
                    Endpoint = endpoint,
                    IPAddress = ip,
                    OldValues = entry.State == EntityState.Added ? null :
                                System.Text.Json.JsonSerializer.Serialize(
                                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)),
                    NewValues = entry.State == EntityState.Deleted ? null :
                                System.Text.Json.JsonSerializer.Serialize(
                                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),
                    CreatedAt = DateTime.UtcNow
                };
                db.AuditLogs.Add(audit);
            }
            await db.SaveChangesAsync();
        }
    }
}
