using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CostCalculationDtos
{
    public class UpdateDiscountRequest
    {
        public decimal DiscountAmount { get; set; }
        public string? Notes { get; set; }
    }
}

