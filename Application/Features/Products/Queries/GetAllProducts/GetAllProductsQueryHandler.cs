using Application.DTOs.Products;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Products.Queries.GetAllProducts
{
    internal class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ResponseViewModel<IEnumerable<ProductDto>>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllProductsQueryHandler> _logger;

        public GetAllProductsQueryHandler(
            IGeneralRepository<Product> repository,
            IMapper mapper,
            ILogger<GetAllProductsQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<IEnumerable<ProductDto>>> Handle(
            GetAllProductsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching products – Page:{Page} Size:{Size} Search:{Search} Category:{Category}",
                request.PageNumber, request.PageSize, request.Search, request.Category);

            var query = _repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(p =>
                    p.Name.Contains(request.Search) ||
                    p.Description.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Category))
                query = query.Where(p => p.Category == request.Category);

            if (!string.IsNullOrWhiteSpace(request.CountryOfOrigin))
                query = query.Where(p => p.CountryOfOrigin == request.CountryOfOrigin);

            // Projection with AutoMapper — no Include needed
            var products = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .AsEnumerable();

            _logger.LogInformation("Products fetched successfully.");

            return ResponseViewModel<IEnumerable<ProductDto>>.Success(products);
        }
    }
}