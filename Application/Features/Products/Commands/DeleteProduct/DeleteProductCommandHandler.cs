using Application.ViewModel;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler
        : IRequestHandler<DeleteProductCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(
            IGeneralRepository<Product> repository,
            ILogger<DeleteProductCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteProductCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Soft-deleting product {ProductId}", request.Id);

            var product = await _repository.GetByIdAsync(request.Id);

            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found for deletion.", request.Id);
                throw new BusinessLogicException(
       $"Product with id '{request.Id}' was not found.",
       "Products",
       AppErrorCode.NotFound);
            }

            // Soft-delete via GeneralRepository
            await _repository.DeleteAsync(request.Id);

            _logger.LogInformation("Product {ProductId} soft-deleted successfully.", request.Id);

            return ResponseViewModel<bool>.Success(true, "Product deleted successfully.");
        }
    }
}