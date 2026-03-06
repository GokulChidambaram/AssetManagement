using AssetManagement.Data;
using AssetManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManagement.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, ApplicationDbContext db)
        {
            // 1. Capture user and request metadata
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = context.User.Identity?.Name;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
            var endpoint = context.Request.Path;
            var method = context.Request.Method;
            var ip = context.Connection.RemoteIpAddress?.ToString();

            // 2. Execute the request
            await _next(context);

            // 3. LOGIC FOR "GET" REQUESTS (READS)
            if (method == "GET")
            {
                var readLog = new AuditLog
                {
                    UserID = int.TryParse(userId, out int id) ? id : null,
                    UserName = userName,
                    Role = role,
                    Action = "READ",
                    EntityName = "System", // Or parse path to determine entity
                    Endpoint = endpoint,
                    HttpMethod = method,
                    IPAddress = ip,
                    CreatedAt = DateTime.UtcNow
                };
                db.AuditLogs.Add(readLog);
                await db.SaveChangesAsync();
                return;
            }

            // 4. LOGIC FOR "POST/PUT/DELETE" (Enriching DB Changes)
            var latestMutationLogs = db.ChangeTracker.Entries<AuditLog>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity);

            foreach (var log in latestMutationLogs)
            {
                log.UserID = int.TryParse(userId, out int id) ? id : null;
                log.UserName = userName;
                log.Role = role;
                log.HttpMethod = method;
                log.Endpoint = endpoint;
                log.IPAddress = ip;
            }

            await db.SaveChangesAsync();
        }
    }
}