using System;

namespace Domain.Models
{
    public enum NotificationType
    {
        ShipmentUpdate = 0,   // تحديث مرحلة الشحنة
        PaymentSuccess = 1,
        PaymentFailed = 2,
        NewMessage = 3,
        CustomsUpdate = 4,
        RequestApproved = 5,
        RequestRejected = 6,
        ContainerUpdate = 7,
        General = 8
    }

    /// <summary>
    /// الإشعارات - تُنشأ تلقائياً عند أي حدث مهم في النظام
    /// RelatedEntityId + RelatedEntityType تسمح بالانتقال للكيان المرتبط
    /// </summary>
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // رابط للكيان المرتبط (ImportRequest، Payment، Message ...)
        public string? RelatedEntityType { get; set; }   // "ImportRequest" / "Payment" / ...
        public string? RelatedEntityId { get; set; }      // GUID as string

        // رابط داخل التطبيق عند الضغط على الإشعار
        public string? ActionUrl { get; set; }

        // Navigation Property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
