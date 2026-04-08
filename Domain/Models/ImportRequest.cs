using Domain.Enums.Enums_Models;
using System;
using System.Collections.Generic;

namespace Domain.Models
{
 

 

  
    public class ImportRequest : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid ProductId { get; set; }

        public Guid? AssignedOfficeId { get; set; }

        public int Quantity { get; set; }
        public decimal TotalWeightKg { get; set; }   // Quantity × WeightPerUnit
        public decimal TotalVolumeCbm { get; set; }  // Quantity × VolumePerUnit

        public ShipmentType ShipmentType { get; set; } = ShipmentType.FullContainer;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public string ShippingAddress { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RequestedDeliveryDate { get; set; }

        // ── Navigation Properties ────────────────────────────────────────

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ApplicationUser? AssignedOffice { get; set; }

        public virtual Product Product { get; set; } = null!;

        // 1-to-1
        public virtual Tracking? Tracking { get; set; }
        public virtual CustomsClearance? CustomsClearance { get; set; }
        public virtual CostCalculation? CostCalculation { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public virtual ContainerItem? ContainerItem { get; set; }
    }
}