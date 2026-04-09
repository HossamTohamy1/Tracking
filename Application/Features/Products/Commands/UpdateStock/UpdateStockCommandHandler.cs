using Application.ViewModel;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Commands.UpdateStock
{
    public class UpdateStockCommandHandler
        : IRequestHandler<UpdateStockCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly ILogger<UpdateStockCommandHandler> _logger;

        public UpdateStockCommandHandler(
            IGeneralRepository<Product> repository,
            ILogger<UpdateStockCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            UpdateStockCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating stock for product {ProductId} to {Qty}",
                request.ProductId, request.NewStockQuantity);

            var product = await _repository.GetByIdAsync(request.ProductId);

            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found for stock update.", request.ProductId);
                throw new BusinessLogicException(
       $"Product with id '{request.ProductId}' was not found.",
       "Products",
       AppErrorCode.NotFound);
            }

            product.StockQuantity = request.NewStockQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdatePartialAsync(product,
                nameof(Product.StockQuantity),
                nameof(Product.UpdatedAt));

            _logger.LogInformation("Stock updated for product {ProductId}.", request.ProductId);

            return ResponseViewModel<bool>.Success(true, "Stock updated successfully.");
        }
    }
}