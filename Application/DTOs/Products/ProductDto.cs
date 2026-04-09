namespace Application.DTOs.Products
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CountryOfOrigin { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal WeightPerUnitKg { get; set; }
        public decimal VolumePerUnitCbm { get; set; }
        public int MinOrderQuantity { get; set; }
        public int StockQuantity { get; set; }
        public string? MainImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}