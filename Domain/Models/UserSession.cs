using System;

namespace Domain.Models
{
    /// <summary>
    /// يخزن Refresh Token لكل جلسة تسجيل دخول
    /// يسمح بتعدد الجلسات لنفس المستخدم (multi-device)
    /// </summary>
    public class UserSession : BaseEntity
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }   // Logout / NewLogin / Suspicious
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        // Navigation Property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
