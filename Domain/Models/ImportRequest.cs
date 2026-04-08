using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public enum RequestStatus
    {
        Pending = 0,        // في انتظار مراجعة المكتب
        Approved = 1,       // تمت الموافقة من المكتب
        Rejected = 2,       // مرفوض من المكتب
        Processing = 3,     // جاري التجهيز
        Shipped = 4,        // تم الشحن
        Customs = 5,        // في التخليص الجمركي
        OutForDelivery = 6,
        Delivered = 7,
        Cancelled = 8       // ملغي من المستخدم
    }

    public enum ShipmentType
    {
        /// <summary>شحنة كبيرة تأخذ كونتنر كامل</summary>
        FullContainer = 0,

        /// <summary>شحنة صغيرة تُجمَّع مع شحنات أخرى في كونتنر مشترك</summary>
        LCL = 1
    }

    /// <summary>
    /// طلب الاستيراد — قلب النظام
    ///
    /// User       → Customer (الطالب)
    /// AssignedOffice → ApplicationUser بدور ImportOffice
    ///
    /// يرتبط بـ: Tracking + CustomsClearance + CostCalculation + Payments
    /// الشحنة الصغيرة (LCL): تُربط بـ ContainerItem → Container
    /// </summary>
    public class ImportRequest : BaseEntity
    {
        /// <summary>العميل صاحب الطلب — Role = Customer</summary>
        public Guid UserId { get; set; }

        public Guid ProductId { get; set; }

        /// <summary>المكتب المكلَّف — Role = ImportOffice (يُعيَّن من Admin أو المكتب نفسه)</summary>
        public Guid? AssignedOfficeId { get; set; }

        public int Quantity { get; set; }
        public decimal TotalWeightKg { get; set; }   // Quantity × WeightPerUnit
        public decimal TotalVolumeCbm { get; set; }  // Quantity × VolumePerUnit

        public ShipmentType ShipmentType { get; set; } = ShipmentType.FullContainer;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public string ShippingAddress { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RequestedDeliveryDate { get; set; }

        // ── Navigation Properties ────────────────────────────────────────

        /// <summary>العميل — ApplicationUser بدور Customer</summary>
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>المكتب المكلَّف — ApplicationUser بدور ImportOffice</summary>
        public virtual ApplicationUser? AssignedOffice { get; set; }

        public virtual Product Product { get; set; } = null!;

        // 1-to-1
        public virtual Tracking? Tracking { get; set; }
        public virtual CustomsClearance? CustomsClearance { get; set; }
        public virtual CostCalculation? CostCalculation { get; set; }

        // 1-to-many: دفعات (جزئية أو كاملة)
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // للشحنات الصغيرة — طلب واحد في كونتنر واحد فقط
        public virtual ContainerItem? ContainerItem { get; set; }
    }
}