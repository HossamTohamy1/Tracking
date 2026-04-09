using Application.DTOs.Products;
using Application.ViewModel;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ResponseViewModel<ProductDto>>
    {
        public Guid Id { get; set; }

        public GetProductByIdQuery(Guid id) => Id = id;
    }
}