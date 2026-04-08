using System;

namespace Domain.Models
{

    public class AuditLog : BaseEntity
    {
        public Guid? UserId { get; set; }

        public string EntityName { get; set; } = string.Empty;   // "ImportRequest" / "Payment" / "User" ...
        public string EntityId { get; set; } = string.Empty;     // GUID as string

        public string Action { get; set; } = string.Empty;       // "Created" / "Updated" / "StatusChanged"

        public string? OldValues { get; set; }   // JSON
        public string? NewValues { get; set; }   // JSON

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ApplicationUser? User { get; set; }
    }
}