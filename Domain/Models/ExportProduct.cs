using System;
using System.Collections.Generic;

namespace Domain.Models
{

    public class ExportProduct : BaseEntity
    {
        public Guid SellerId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CountryOfOrigin { get; set; } = string.Empty;

        public decimal PricePerUnit { get; set; }
        public string Currency { get; set; } = "USD";
        public int MinOrderQuantity { get; set; } = 1;
        public int StockQuantity { get; set; } = 0;

        public decimal WeightPerUnitKg { get; set; }
        public decimal VolumePerUnitCbm { get; set; }

        public string? MainImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation Properties
        public virtual ApplicationUser Seller { get; set; } = null!;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}