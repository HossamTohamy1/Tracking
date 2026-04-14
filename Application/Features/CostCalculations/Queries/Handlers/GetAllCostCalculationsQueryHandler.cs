using Application.DTOs.CostCalculationDtos;
using Application.Features.Containers.Queries;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CostCalculations.Queries.Handlers
{
    public class GetAllCostCalculationsQueryHandler
        : IRequestHandler<GetAllCostCalculationsQuery, ResponseViewModel<PaginatedResult<CostCalculationDto>>>
    {
        private readonly IGeneralRepository<CostCalculation> _costRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllCostCalculationsQueryHandler> _logger;

        public GetAllCostCalculationsQueryHandler(
            IGeneralRepository<CostCalculation> costRepo,
            IMapper mapper,
            ILogger<GetAllCostCalculationsQueryHandler> logger)
        {
            _costRepo = costRepo ?? throw new ArgumentNullException(nameof(costRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<PaginatedResult<CostCalculationDto>>> Handle(
            GetAllCostCalculationsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[GET ALL COST CALCULATIONS] Admin query. Page: {Page}, PageSize: {PageSize}",
                    request.Page,
                    request.PageSize);

                // ── BUILD QUERY ───────────────────────────────────────────────
                var query = _costRepo.GetAll();

                if (!string.IsNullOrWhiteSpace(request.Currency))
                    query = query.Where(x => x.Currency == request.Currency.Trim().ToUpper());

                // ── PAGINATION ────────────────────────────────────────────────
                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ProjectTo<CostCalculationDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                var result = new PaginatedResult<CostCalculationDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                _logger.LogInformation(
                    "[GET ALL COST CALCULATIONS] Retrieved {Count} records. TotalCount: {TotalCount}",
                    items.Count,
                    totalCount);

                return ResponseViewModel<PaginatedResult<CostCalculationDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET ALL COST CALCULATIONS] Unexpected error");

                throw new BusinessLogicException(
                    "Failed to retrieve cost calculations",
                    ex,
                    "CostCalculation");
            }
        }
    }
}