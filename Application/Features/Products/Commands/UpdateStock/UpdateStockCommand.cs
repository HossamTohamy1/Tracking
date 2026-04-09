using Application.ViewModel;
using MediatR;

namespace Application.Features.Products.Commands.UpdateStock
{
    public class UpdateStockCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid ProductId { get; set; }
        public int NewStockQuantity { get; set; }
    }
}