using Application.DTOs.Products;
using Application.ViewModel;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<ResponseViewModel<ProductDto>>
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? CountryOfOrigin { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Currency { get; set; }
        public decimal? WeightPerUnitKg { get; set; }
        public decimal? VolumePerUnitCbm { get; set; }
        public int? MinOrderQuantity { get; set; }
    }
}