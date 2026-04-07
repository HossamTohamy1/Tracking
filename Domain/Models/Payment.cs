using System;

namespace Domain.Models
{
    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4,
        Cancelled = 5
    }

    public enum PaymentMethod
    {
        CreditCard = 0,
        DebitCard = 1,
        PayPal = 2,
        BankTransfer = 3,
        Stripe = 4,
        Cash = 5
    }

    public enum PaymentPurpose
    {
        ImportShipment = 0,   // دفع لشحنة كبيرة (Full Container)
        ContainerShare = 1,   // دفع حصة في كونتنر مشترك (LCL)
        CustomsDuty = 2,      // دفع رسوم جمركية منفصلة
        PartialDeposit = 3    // عربون / دفعة أولى
    }

    /// <summary>
    /// المدفوعات - مرتبط بـ ImportRequest أو Container أو كليهما
    /// نظام الدفعات يدعم الدفع الجزئي: طلب واحد يمكن أن يكون له أكثر من Payment
    /// </summary>
    public class Payment : BaseEntity
    {
        public Guid UserId { get; set; }

        // أحد هذين يجب أن يكون موجوداً حسب نوع الدفع
        public Guid? ImportRequestId { get; set; }
        public Guid? ContainerId { get; set; }          // للـ LCL: الدفع على مستوى الكونتنر

        public PaymentPurpose Purpose { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod Method { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";

        // بيانات البوابة الإلكترونية
        public string? GatewayTransactionId { get; set; }  // Stripe PaymentIntent / PayPal Order ID
        public string? GatewayResponse { get; set; }        // الاستجابة الكاملة (JSON)

        public DateTime? PaidAt { get; set; }
        public string? FailureReason { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ImportRequest? ImportRequest { get; set; }
        public virtual Container? Container { get; set; }
    }
}
