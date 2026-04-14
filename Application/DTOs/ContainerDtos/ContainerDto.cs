using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ContainerDtos
{
    public class ContainerDto
    {
        public Guid Id { get; set; }
        public string ContainerNumber { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public decimal MaxWeightKg { get; set; }
        public decimal MaxVolumeCbm { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal CurrentVolumeCbm { get; set; }

        public decimal WeightUtilizationPercent { get; set; }
        public decimal VolumeUtilizationPercent { get; set; }

        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? ExpectedArrival { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public decimal TotalShippingCost { get; set; }

        public Guid ManagedByOfficeId { get; set; }
        public string ManagedByOfficeName { get; set; } = string.Empty;

        public List<ContainerItemDto> Items { get; set; } = new List<ContainerItemDto>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
    public class ContainerItemDto
    {
        public Guid Id { get; set; }
        public Guid ImportRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;

        public decimal WeightKg { get; set; }
        public decimal VolumeCbm { get; set; }
        public decimal CostShare { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ContainerListItemDto
    {
        public Guid Id { get; set; }
        public string ContainerNumber { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public decimal CurrentWeightKg { get; set; }
        public decimal MaxWeightKg { get; set; }
        public decimal CurrentVolumeCbm { get; set; }
        public decimal MaxVolumeCbm { get; set; }

        public int ItemCount { get; set; }
        public decimal TotalShippingCost { get; set; }

        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ExpectedArrival { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
