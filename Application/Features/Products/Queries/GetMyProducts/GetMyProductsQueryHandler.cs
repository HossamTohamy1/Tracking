using Application.DTOs.Products;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Application.Features.Products.Queries.GetMyProducts
{
    public class GetMyProductsQueryHandler
        : IRequestHandler<GetMyProductsQuery, ResponseViewModel<IEnumerable<ProductDto>>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetMyProductsQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetMyProductsQueryHandler(
            IGeneralRepository<Product> repository,
            IMapper mapper,
            ILogger<GetMyProductsQueryHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseViewModel<IEnumerable<ProductDto>>> Handle(
            GetMyProductsQuery request,
            CancellationToken cancellationToken)
        {
            var officeIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Office {OfficeId} fetching their products.", officeIdClaim);

            // ⚠️ If you add OwnerId (Guid) to the Product entity, replace the Where below with:
            //    .Where(p => p.OwnerId.ToString() == officeIdClaim)
            // For now we return all active products scoped to the office via HTTP context.
            var products = _repository.GetAll()
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .AsEnumerable();

            _logger.LogInformation("Returned {Count} products for office {OfficeId}.",
                products.Count(), officeIdClaim);

            return ResponseViewModel<IEnumerable<ProductDto>>.Success(products);
        }
    }
}