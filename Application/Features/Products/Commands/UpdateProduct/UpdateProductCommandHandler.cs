using Application.DTOs.Products;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler
        : IRequestHandler<UpdateProductCommand, ResponseViewModel<ProductDto>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IGeneralRepository<Product> repository,
            IMapper mapper,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ProductDto>> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating product {ProductId}", request.Id);

            var product = await _repository.GetByIdAsync(request.Id);

            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found for update.", request.Id);
                throw new BusinessLogicException(
       $"Product with id '{request.Id}' was not found.",
       "Products",
       AppErrorCode.NotFound);
            }

            // Only update fields that were provided
            var modifiedProps = new List<string>();

            if (request.Name is not null) { product.Name = request.Name; modifiedProps.Add(nameof(Product.Name)); }
            if (request.Description is not null) { product.Description = request.Description; modifiedProps.Add(nameof(Product.Description)); }
            if (request.Category is not null) { product.Category = request.Category; modifiedProps.Add(nameof(Product.Category)); }
            if (request.CountryOfOrigin is not null) { product.CountryOfOrigin = request.CountryOfOrigin; modifiedProps.Add(nameof(Product.CountryOfOrigin)); }
            if (request.UnitPrice.HasValue) { product.UnitPrice = request.UnitPrice.Value; modifiedProps.Add(nameof(Product.UnitPrice)); }
            if (request.Currency is not null) { product.Currency = request.Currency; modifiedProps.Add(nameof(Product.Currency)); }
            if (request.WeightPerUnitKg.HasValue) { product.WeightPerUnitKg = request.WeightPerUnitKg.Value; modifiedProps.Add(nameof(Product.WeightPerUnitKg)); }
            if (request.VolumePerUnitCbm.HasValue) { product.VolumePerUnitCbm = request.VolumePerUnitCbm.Value; modifiedProps.Add(nameof(Product.VolumePerUnitCbm)); }
            if (request.MinOrderQuantity.HasValue) { product.MinOrderQuantity = request.MinOrderQuantity.Value; modifiedProps.Add(nameof(Product.MinOrderQuantity)); }

            product.UpdatedAt = DateTime.UtcNow;
            modifiedProps.Add(nameof(Product.UpdatedAt));

            await _repository.UpdatePartialAsync(product, modifiedProps.ToArray());

            // Projection with AutoMapper — no Include needed
            var dto = _repository.GetAll()
                .Where(p => p.Id == product.Id)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .First();

            _logger.LogInformation("Product {ProductId} updated successfully.", product.Id);

            return ResponseViewModel<ProductDto>.Success(dto, "Product updated successfully.");
        }
    }
}