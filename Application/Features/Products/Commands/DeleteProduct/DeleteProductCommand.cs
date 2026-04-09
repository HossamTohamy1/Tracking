using Application.ViewModel;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeleteProductCommand(Guid id) => Id = id;
    }
}