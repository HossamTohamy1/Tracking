using Application.DTOs.Products;
using Application.ViewModel;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<ResponseViewModel<ProductDto>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CountryOfOrigin { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal WeightPerUnitKg { get; set; }
        public decimal VolumePerUnitCbm { get; set; }
        public int MinOrderQuantity { get; set; } = 1;
        public int StockQuantity { get; set; } = 0;
    }
}