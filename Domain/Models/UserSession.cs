using System;

namespace Domain.Models
{
    /// <summary>
    /// جلسات المستخدم — تُسجَّل عند كل Login
    /// تُستخدم لإدارة الـ Active Sessions وإمكانية Revoke Token
    /// </summary>
    public class UserSession : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Token { get; set; } = string.Empty;          // JWT Token (أو Hash منه)
        public string? RefreshToken { get; set; }                   // للتجديد التلقائي لاحقاً
        public DateTime ExpiresAt { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }                      // المتصفح / الجهاز
        public string? DeviceInfo { get; set; }

        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }                  // "Logout" | "Admin" | "Expired"

        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}