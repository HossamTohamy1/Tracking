using System;

namespace Domain.Models
{

    public class UserSession : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Token { get; set; } = string.Empty;        
        public string? RefreshToken { get; set; }                  
        public DateTime ExpiresAt { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }                      
        public string? DeviceInfo { get; set; }

        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }                  

        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}