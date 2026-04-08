using System;

namespace Domain.Models
{
    public enum ReviewTargetType
    {
        ImportOffice = 0,   // تقييم مكتب استيراد بعد اكتمال شحنة
        ExportProduct = 1   // تقييم منتج قابل للتصدير
    }

    /// <summary>
    /// التقييمات — يدعم تقييم مكاتب الاستيراد والمنتجات
    ///
    /// Reviewer   → Customer (أتمّ معاملة فعلية)
    /// Office     → ApplicationUser بدور ImportOffice
    /// Exporter   → ApplicationUser بدور Exporter (عبر ExportProduct)
    ///
    /// IsVerified = true لو المُقيِّم أتمّ شحنة/صفقة فعلية
    /// </summary>
    public class Review : BaseEntity
    {
        /// <summary>كاتب التقييم — Role = Customer</summary>
        public Guid ReviewerId { get; set; }

        public ReviewTargetType TargetType { get; set; }

        /// <summary>المكتب المُقيَّم — Role = ImportOffice (يُملأ لو TargetType = ImportOffice)</summary>
        public Guid? OfficeId { get; set; }

        /// <summary>المنتج المُقيَّم (يُملأ لو TargetType = ExportProduct)</summary>
        public Guid? ExportProductId { get; set; }

        /// <summary>الطلب المرتبط — للتحقق من المعاملة الفعلية</summary>
        public Guid? ImportRequestId { get; set; }

        public int Rating { get; set; }   // 1 - 5
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        /// <summary>هل أتمّ المُقيِّم معاملة فعلية؟ (Verified Purchase)</summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>رد المكتب أو المُصدِّر على التقييم</summary>
        public string? ReplyText { get; set; }
        public DateTime? RepliedAt { get; set; }

        // Navigation Properties
        /// <summary>كاتب التقييم — ApplicationUser بدور Customer</summary>
        public virtual ApplicationUser Reviewer { get; set; } = null!;

        /// <summary>المكتب المُقيَّم — ApplicationUser بدور ImportOffice</summary>
        public virtual ApplicationUser? Office { get; set; }

        public virtual ExportProduct? ExportProduct { get; set; }
        public virtual ImportRequest? ImportRequest { get; set; }
    }
}