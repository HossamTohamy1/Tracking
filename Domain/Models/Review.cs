using System;

namespace Domain.Models
{
    public enum ReviewTargetType
    {
        Office = 0,         // تقييم مكتب بعد اكتمال شحنة
        ExportProduct = 1   // تقييم منتج قابل للتصدير
    }

    /// <summary>
    /// التقييمات - نمط عام يدعم تقييم المكاتب والمنتجات
    /// IsVerified = true عندما يكون المُقيِّم قد أتمّ فعلاً معاملة مع الهدف
    /// </summary>
    public class Review : BaseEntity
    {
        public Guid ReviewerId { get; set; }
        public ReviewTargetType TargetType { get; set; }

        // أحد هذين يُملأ حسب TargetType
        public Guid? OfficeId { get; set; }           // عند تقييم مكتب
        public Guid? ExportProductId { get; set; }    // عند تقييم منتج

        // الطلب الذي بني عليه التقييم (للتحقق من المشتريات الفعلية)
        public Guid? ImportRequestId { get; set; }

        public int Rating { get; set; }               // 1 - 5
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        // هل المُقيِّم أتمّ معاملة فعلية؟ (Verified Purchase)
        public bool IsVerified { get; set; } = false;

        // رد المكتب أو البائع على التقييم
        public string? ReplyText { get; set; }
        public DateTime? RepliedAt { get; set; }

        // Navigation Properties
        public virtual ApplicationUser Reviewer { get; set; } = null!;
        public virtual ApplicationUser? Office { get; set; }
        public virtual ExportProduct? ExportProduct { get; set; }
        public virtual ImportRequest? ImportRequest { get; set; }
    }
}
