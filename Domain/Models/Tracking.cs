using Domain.Enums.Enums_Models;
using System;
using System.Collections.Generic;

namespace Domain.Models
{


    public class Tracking : BaseEntity
    {
        public Guid ImportRequestId { get; set; }
        public ShipmentStage CurrentStage { get; set; } = ShipmentStage.Purchased;
        public string? TrackingNumber { get; set; }
        public string? CarrierName { get; set; }
        public string? CurrentLocation { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        public DateTime? ShippedAt { get; set; }
        public DateTime? ArrivedPortAt { get; set; }
        public DateTime? CustomsClearedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Navigation Properties
        public virtual ImportRequest ImportRequest { get; set; } = null!;
        public virtual ICollection<TrackingHistory> History { get; set; } = new List<TrackingHistory>();
    }

    /// <summary>
    /// سجل كل تغيير في حالة التتبع (Audit Trail للشحنة)
    /// UpdatedBy → ImportOffice أو Admin
    /// </summary>
    public class TrackingHistory : BaseEntity
    {
        public Guid TrackingId { get; set; }
        public ShipmentStage Stage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>من قام بتحديث الحالة — ImportOffice أو Admin</summary>
        public Guid? UpdatedByUserId { get; set; }

        // Navigation Properties
        public virtual Tracking Tracking { get; set; } = null!;

        /// <summary>ApplicationUser بدور ImportOffice أو Admin</summary>
        public virtual ApplicationUser? UpdatedBy { get; set; }
    }
}