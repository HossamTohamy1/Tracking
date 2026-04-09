using Application.DTOs.Products;
using Application.ViewModel;
using MediatR;

namespace Application.Features.Products.Queries.GetMyProducts
{
    public class GetMyProductsQuery : IRequest<ResponseViewModel<IEnumerable<ProductDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}