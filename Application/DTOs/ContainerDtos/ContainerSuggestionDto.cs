using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ContainerDtos
{

    public class ContainerSuggestionDto
    {
        public Guid ContainerId { get; set; }
        public string ContainerNumber { get; set; } = string.Empty;
        public decimal AvailableWeightKg { get; set; }
        public decimal AvailableVolumeCbm { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal CurrentVolumeCbm { get; set; }
        public decimal MaxWeightKg { get; set; }
        public decimal MaxVolumeCbm { get; set; }
        public string? DestinationPort { get; set; }
        public string? OriginPort { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalShippingCost { get; set; }
        public int Score { get; set; }          // 0–100
        public bool IsBestMatch { get; set; }
        public decimal WeightUtilizationAfter { get; set; }   // % after adding
        public decimal VolumeUtilizationAfter { get; set; }   // % after adding
    }
}