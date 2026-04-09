using Application.DTOs.Products;
using Application.ViewModel;
using MediatR;

namespace Application.Features.Products.Queries.GetAllProducts
{
    public class GetAllProductsQuery : IRequest<ResponseViewModel<IEnumerable<ProductDto>>>
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? CountryOfOrigin { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}