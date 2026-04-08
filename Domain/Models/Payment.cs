using Domain.Enums.Enums_Models;
using System;

namespace Domain.Models
{
  






    public class Payment : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid? ImportRequestId { get; set; }
        public Guid? ContainerId { get; set; }          

        public PaymentPurpose Purpose { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod Method { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";

        public string? GatewayTransactionId { get; set; }  
        public string? GatewayResponse { get; set; }       

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
