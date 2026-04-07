using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public enum ContainerStatus
    {
        Open = 0,          // مفتوح - يقبل شحنات جديدة
        Closed = 1,        // مغلق - لا يقبل شحنات جديدة
        Shipped = 2,       // تم شحنه
        InTransit = 3,
        ArrivedPort = 4,
        Customs = 5,
        Delivered = 6,
        Cancelled = 7
    }

    /// <summary>
    /// الكونتنر المشترك - يجمع شحنات صغيرة (LCL) متعددة
    /// يديره المكتب (Office)
    /// تكلفة الشحن تُوزَّع على الشحنات بحسب الوزن أو الحجم
    /// </summary>
    public class Container : BaseEntity
    {
        public string ContainerNumber { get; set; } = string.Empty;  // UNIQUE
        public ContainerStatus Status { get; set; } = ContainerStatus.Open;

        public decimal MaxWeightKg { get; set; }
        public decimal MaxVolumeCbm { get; set; }

        // يُحدَّث تلقائياً عند إضافة/إزالة ContainerItem
        public decimal CurrentWeightKg { get; set; } = 0;
        public decimal CurrentVolumeCbm { get; set; } = 0;

        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? ExpectedArrival { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // تكلفة الشحن الإجمالية للكونتنر (تُوزَّع على الشحنات)
        public decimal TotalShippingCost { get; set; } = 0;

        // المكتب المسؤول عن الكونتنر
        public Guid ManagedByOfficeId { get; set; }
        public virtual ApplicationUser ManagedByOffice { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<ContainerItem> Items { get; set; } = new List<ContainerItem>();
    }

    /// <summary>
    /// جدول الربط بين ImportRequest والContainer
    /// يخزن حصة كل شحنة من وزن وحجم وتكلفة الكونتنر
    /// </summary>
    public class ContainerItem : BaseEntity
    {
        public Guid ContainerId { get; set; }
        public Guid ImportRequestId { get; set; }   // UNIQUE - كل طلب في كونتنر واحد فقط

        public decimal WeightKg { get; set; }
        public decimal VolumeCbm { get; set; }

        // الحصة المحسوبة من تكلفة الشحن الإجمالية للكونتنر
        // تُحسب: (Volume / TotalVolume) × TotalShippingCost
        public decimal CostShare { get; set; } = 0;

        // Navigation Properties
        public virtual Container Container { get; set; } = null!;
        public virtual ImportRequest ImportRequest { get; set; } = null!;
    }
}
