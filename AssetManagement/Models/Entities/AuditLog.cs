namespace AssetManagement.Models.Entities
{
    public class AuditLog

    {

        public int AuditLogID { get; set; }

        public int? UserID { get; set; }     // ✅ ADD THIS

        public string? UserName { get; set; }

        public string? Role { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public string? EntityID { get; set; }

        public string HttpMethod { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;

        public string? IPAddress { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

}
