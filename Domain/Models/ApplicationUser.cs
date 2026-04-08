using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
   
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }


        public string? CompanyName { get; set; }

        public string? Address { get; set; }

        public string? Country { get; set; }

    
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }


        public virtual ICollection<ImportRequest> MyImportRequests { get; set; } = new List<ImportRequest>();

        public virtual ICollection<ImportRequest> AssignedRequests { get; set; } = new List<ImportRequest>();

        public virtual ICollection<Container> ManagedContainers { get; set; } = new List<Container>();

        public virtual ICollection<CustomsClearance> HandledClearances { get; set; } = new List<CustomsClearance>();

        public virtual ICollection<ExportProduct> ExportProducts { get; set; } = new List<ExportProduct>();

        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public virtual ICollection<Review> WrittenReviews { get; set; } = new List<Review>();

        public virtual ICollection<Review> ReceivedReviews { get; set; } = new List<Review>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public virtual ICollection<CustomsDocument> UploadedDocuments { get; set; } = new List<CustomsDocument>();
    }
}