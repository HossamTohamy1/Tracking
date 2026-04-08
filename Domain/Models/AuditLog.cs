using System;

namespace Domain.Models
{
    /// <summary>
    /// سجل التدقيق — يُسجَّل تلقائياً عند أي تغيير على كيانات حساسة
    ///
    /// UserId nullable: إذا كان التغيير من النظام تلقائياً
    /// Action: "Created" | "Updated" | "Deleted" | "StatusChanged" | "RoleChanged"
    /// OldValues / NewValues: JSON للمقارنة
    ///
    /// يُستخدم من: Admin و Support للمراجعة والتدقيق
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>null = النظام تلقائياً | Guid = Admin أو ImportOffice أو أي Role</summary>
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