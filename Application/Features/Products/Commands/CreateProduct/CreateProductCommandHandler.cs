using Application.DTOs.Products;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler
        : IRequestHandler<CreateProductCommand, ResponseViewModel<ProductDto>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IGeneralRepository<Product> repository,
            IMapper mapper,
            ILogger<CreateProductCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ProductDto>> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new product: {ProductName}", request.Name);

            var product = _mapper.Map<Product>(request);

            await _repository.AddAsync(product);

            // Projection with AutoMapper — no Include needed
            var dto = _repository.GetAll()
                .Where(p => p.Id == product.Id)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .First();

            _logger.LogInformation("Product {ProductId} created successfully.", product.Id);

            return ResponseViewModel<ProductDto>.Success(dto, "Product created successfully.");
        }
    }
}