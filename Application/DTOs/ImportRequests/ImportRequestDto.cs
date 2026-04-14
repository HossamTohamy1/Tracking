using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ImportRequests
{
    public class ImportRequestDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid? AssignedOfficeId { get; set; }
        public string? AssignedOfficeName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalWeightKg { get; set; }
        public decimal TotalVolumeCbm { get; set; }
        public string ShipmentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RequestedDeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}