using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CostCalculationDtos
{
    public class CostCalculationDto
    {
        public Guid Id { get; set; }
        public Guid ImportRequestId { get; set; }

        // Shipment snapshot
        public decimal WeightKg { get; set; }
        public decimal VolumeCbm { get; set; }

        // Cost components
        public decimal BaseShippingCost { get; set; }
        public decimal CustomsDuty { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InsuranceCost { get; set; }
        public decimal HandlingFee { get; set; }
        public decimal OtherFees { get; set; }
        public decimal DiscountAmount { get; set; }

        // Totals
        public decimal TotalBeforeDiscount { get; set; }
        public decimal FinalAmount { get; set; }

        public string Currency { get; set; } = "USD";
        public string? Notes { get; set; }

        // From ImportRequest projection (no Include)
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public bool IsLocked { get; set; }   // true when a Completed payment exists

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}