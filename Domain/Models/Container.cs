using Domain.Enums.Enums_Models;
using System;
using System.Collections.Generic;

namespace Domain.Models
{



    public class Container : BaseEntity
    {
        public string ContainerNumber { get; set; } = string.Empty;  // UNIQUE
        public ContainerStatus Status { get; set; } = ContainerStatus.Open;

        public decimal MaxWeightKg { get; set; }
        public decimal MaxVolumeCbm { get; set; }

        public decimal CurrentWeightKg { get; set; } = 0;
        public decimal CurrentVolumeCbm { get; set; } = 0;

        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? ExpectedArrival { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public decimal TotalShippingCost { get; set; } = 0;

        public Guid ManagedByOfficeId { get; set; }
        public virtual ApplicationUser ManagedByOffice { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<ContainerItem> Items { get; set; } = new List<ContainerItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }


    public class ContainerItem : BaseEntity
    {
        public Guid ContainerId { get; set; }
        public Guid ImportRequestId { get; set; }   

        public decimal WeightKg { get; set; }
        public decimal VolumeCbm { get; set; }

        public decimal CostShare { get; set; } = 0;

        // Navigation Properties
        public virtual Container Container { get; set; } = null!;
        public virtual ImportRequest ImportRequest { get; set; } = null!;
    }
}