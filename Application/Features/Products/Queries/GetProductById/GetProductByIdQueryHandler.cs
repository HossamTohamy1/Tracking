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

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler
        : IRequestHandler<GetProductByIdQuery, ResponseViewModel<ProductDto>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;

        public GetProductByIdQueryHandler(
            IGeneralRepository<Product> repository,
            IMapper mapper,
            ILogger<GetProductByIdQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ProductDto>> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching product {ProductId}", request.Id);

            // Projection with AutoMapper — no Include needed
            var product = _repository.GetAll()
                .Where(p => p.Id == request.Id)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault();

            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found.", request.Id);
                throw new BusinessLogicException(
                    $"Product with id '{request.Id}' was not found.",
                    "Products",
                    AppErrorCode.NotFound);
            }

            _logger.LogInformation("Product {ProductId} fetched successfully.", request.Id);

            return ResponseViewModel<ProductDto>.Success(product);
        }
    }
}