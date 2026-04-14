using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CostCalculationDtos
{
    public class UpdateCostCalculationRequest
    {
        public decimal BaseShippingCost { get; set; }
        public decimal CustomsDuty { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InsuranceCost { get; set; }
        public decimal HandlingFee { get; set; }
        public decimal OtherFees { get; set; }
        public string? Notes { get; set; }
        public string? Currency { get; set; }
    }
}
