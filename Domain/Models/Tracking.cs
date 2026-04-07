using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public enum ShipmentStage
    {
        Purchased = 0,
        Processing = 1,    // جاري التجهيز
        ReadyToShip = 2,
        Shipped = 3,       // تم الشحن / على الباخرة
        InTransit = 4,     // في الطريق
        ArrivedPort = 5,   // وصل الميناء
        Customs = 6,       // في التخليص الجمركي
        OutForDelivery = 7,
        Delivered = 8,
        Exception = 9      // مشكلة / توقف
    }

    /// <summary>
    /// تتبع الشحنة - يُنشأ تلقائياً عند الموافقة على الطلب
    /// يحتفظ بالمرحلة الحالية + سجل كامل بالتغييرات
    /// </summary>
    public class Tracking : BaseEntity
    {
        public Guid ImportRequestId { get; set; }
        public ShipmentStage CurrentStage { get; set; } = ShipmentStage.Purchased;
        public string? TrackingNumber { get; set; }
        public string? CarrierName { get; set; }
        public string? CurrentLocation { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        // تواريخ المراحل الرئيسية (تُملأ عند الوصول لكل مرحلة)
        public DateTime? ShippedAt { get; set; }
        public DateTime? ArrivedPortAt { get; set; }
        public DateTime? CustomsClearedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Navigation Properties
        public virtual ImportRequest ImportRequest { get; set; } = null!;
        public virtual ICollection<TrackingHistory> History { get; set; } = new List<TrackingHistory>();
    }

    /// <summary>
    /// سجل كل تغيير في حالة التتبع (Audit Trail للشحنة)
    /// </summary>
    public class TrackingHistory : BaseEntity
    {
        public Guid TrackingId { get; set; }
        public ShipmentStage Stage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        // من قام بتحديث الحالة (Office أو Admin)
        public Guid? UpdatedByUserId { get; set; }

        // Navigation Properties
        public virtual Tracking Tracking { get; set; } = null!;
        public virtual ApplicationUser? UpdatedBy { get; set; }
    }
}
