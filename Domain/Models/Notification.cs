using System;

namespace Domain.Models
{
    public enum NotificationType
    {
        ShipmentUpdate = 0,   
        PaymentSuccess = 1,
        PaymentFailed = 2,
        NewMessage = 3,
        CustomsUpdate = 4,
        RequestApproved = 5,
        RequestRejected = 6,
        ContainerUpdate = 7,
        General = 8
    }


    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public string? RelatedEntityType { get; set; }  
        public string? RelatedEntityId { get; set; }   

        public string? ActionUrl { get; set; }

        // Navigation Property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
