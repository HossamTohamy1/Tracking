using System;

namespace Domain.Models
{
    /// <summary>
    /// سجل التدقيق - يُسجَّل تلقائياً عند أي تغيير على كيانات حساسة
    /// UserId nullable: إذا كان التغيير من النظام تلقائياً وليس من مستخدم
    /// OldValues / NewValues: JSON للمقارنة
    /// </summary>
    public class AuditLog : BaseEntity
    {
        // null = النظام (System/Scheduler)
        public Guid? UserId { get; set; }

        public string EntityName { get; set; } = string.Empty;    // "ImportRequest" / "Payment" ...
        public string EntityId { get; set; } = string.Empty;      // GUID as string

        public string Action { get; set; } = string.Empty;        // "Created" / "Updated" / "Deleted" / "StatusChanged"

        public string? OldValues { get; set; }   // JSON
        public string? NewValues { get; set; }   // JSON

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ApplicationUser? User { get; set; }
    }
}
