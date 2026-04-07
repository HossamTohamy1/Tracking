using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public enum RequestStatus
    {
        Pending = 0,       // في انتظار مراجعة المكتب
        Approved = 1,      // تمت الموافقة من المكتب
        Rejected = 2,      // مرفوض من المكتب
        Processing = 3,    // جاري التجهيز
        Shipped = 4,       // تم الشحن
        Customs = 5,       // في التخليص الجمركي
        OutForDelivery = 6,
        Delivered = 7,
        Cancelled = 8      // ملغي من المستخدم
    }

    public enum ShipmentType
    {
        /// <summary>شحنة كبيرة تأخذ كونتنر كامل</summary>
        FullContainer = 0,

        /// <summary>شحنة صغيرة تُجمَّع مع شحنات أخرى في كونتنر مشترك</summary>
        LCL = 1   // Less than Container Load
    }

    /// <summary>
    /// طلب الاستيراد - قلب النظام
    /// يرتبط بـ: User (الطالب) + Office (المكلَّف) + Product + Tracking + CustomsClearance
    /// الدفع: Payment مرتبط بطلب الاستيراد مباشرة
    /// الشحنة الصغيرة: تُربط بـ ContainerItem → Container
    /// </summary>
    public class ImportRequest : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? AssignedOfficeId { get; set; }   // يُعيَّن من Admin أو Office

        public int Quantity { get; set; }
        public decimal TotalWeightKg { get; set; }    // يُحسب: Quantity × WeightPerUnit
        public decimal TotalVolumeCbm { get; set; }   // يُحسب: Quantity × VolumePerUnit

        public ShipmentType ShipmentType { get; set; } = ShipmentType.FullContainer;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public string ShippingAddress { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RequestedDeliveryDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationUser? AssignedOffice { get; set; }
        public virtual Product Product { get; set; } = null!;

        // 1-to-1: كل طلب له تتبع واحد
        public virtual Tracking? Tracking { get; set; }

        // 1-to-1: كل طلب له تخليص جمركي واحد
        public virtual CustomsClearance? CustomsClearance { get; set; }

        // 1-to-1: حساب التكلفة (يُنشأ بعد الموافقة على الطلب)
        public virtual CostCalculation? CostCalculation { get; set; }

        // 1-to-many: الدفعات (قد يكون دفع جزئي ثم دفعة نهائية)
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // للشحنات الصغيرة: الطلب يكون ضمن ContainerItem واحد فقط
        public virtual ContainerItem? ContainerItem { get; set; }
    }
}
