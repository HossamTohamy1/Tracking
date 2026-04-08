using System;

namespace Domain.Models
{

    public class CostCalculation : BaseEntity
    {
        public Guid ImportRequestId { get; set; }

        public decimal WeightKg { get; set; }
        public decimal VolumeCbm { get; set; }

        public decimal BaseShippingCost { get; set; }
        public decimal CustomsDuty { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InsuranceCost { get; set; }
        public decimal HandlingFee { get; set; }
        public decimal OtherFees { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        public decimal TotalBeforeDiscount { get; set; }   
        public decimal FinalAmount { get; set; }            

        public string Currency { get; set; } = "USD";
        public string? Notes { get; set; }

        // Navigation Property
        public virtual ImportRequest ImportRequest { get; set; } = null!;
    }
}
