using System;

namespace Domain.Models
{
    /// <summary>
    /// حساب تكلفة الشحنة - يُنشأ بعد الموافقة على الطلب
    /// 1-to-1 مع ImportRequest
    /// FinalAmount = BaseShippingCost + CustomsDuty + TaxAmount + InsuranceCost + HandlingFee - DiscountAmount
    /// </summary>
    public class CostCalculation : BaseEntity
    {
        public Guid ImportRequestId { get; set; }

        // بيانات الشحنة وقت الحساب (قد تتغير لاحقاً لذا نحفظها هنا)
        public decimal WeightKg { get; set; }
        public decimal VolumeCbm { get; set; }

        // تفصيل التكاليف
        public decimal BaseShippingCost { get; set; }
        public decimal CustomsDuty { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InsuranceCost { get; set; }
        public decimal HandlingFee { get; set; }
        public decimal OtherFees { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        // الإجماليات
        public decimal TotalBeforeDiscount { get; set; }   // مجموع كل التكاليف
        public decimal FinalAmount { get; set; }            // بعد الخصم

        public string Currency { get; set; } = "USD";
        public string? Notes { get; set; }

        // Navigation Property
        public virtual ImportRequest ImportRequest { get; set; } = null!;
    }
}
