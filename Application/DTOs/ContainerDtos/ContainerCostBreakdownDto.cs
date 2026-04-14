using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ContainerDtos
{
    public class ContainerCostBreakdownDto
    {
        public Guid ContainerId { get; set; }
        public string ContainerNumber { get; set; } = string.Empty;
        public decimal TotalShippingCost { get; set; }
        public decimal TotalVolumeCbm { get; set; }

        public List<CostShareItemDto> Items { get; set; } = new List<CostShareItemDto>();
    }

    public class CostShareItemDto
    {
        public Guid ContainerItemId { get; set; }
        public Guid ImportRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;

        public decimal VolumeCbm { get; set; }
        public decimal WeightKg { get; set; }

        public decimal VolumePercentage { get; set; }
        public decimal CostShare { get; set; }

        public string CustomerName { get; set; } = string.Empty;
    }
}