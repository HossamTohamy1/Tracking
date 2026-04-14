using Application.DTOs.CostCalculationDtos;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
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
    public class GetCostCalculationByRequestIdQueryHandler
         : IRequestHandler<GetCostCalculationByRequestIdQuery, ResponseViewModel<CostCalculationDto>>
    {
        private readonly IGeneralRepository<CostCalculation> _costRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCostCalculationByRequestIdQueryHandler> _logger;

        public GetCostCalculationByRequestIdQueryHandler(
            IGeneralRepository<CostCalculation> costRepo,
            IMapper mapper,
            ILogger<GetCostCalculationByRequestIdQueryHandler> logger)
        {
            _costRepo = costRepo ?? throw new ArgumentNullException(nameof(costRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<CostCalculationDto>> Handle(
            GetCostCalculationByRequestIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[GET COST CALCULATION] Fetching cost calculation. RequestId: {RequestId}",
                    request.RequestId);

                // ── Use ProjectTo instead of Include ──────────────────────────────
                var dto = await _costRepo.GetAll()
                    .Where(x => x.ImportRequestId == request.RequestId)
                    .ProjectTo<CostCalculationDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dto is null)
                {
                    _logger.LogWarning(
                        "[GET COST CALCULATION] Cost calculation not found. RequestId: {RequestId}",
                        request.RequestId);

                    throw new NotFoundException(
                        $"Cost calculation for request {request.RequestId} not found",
                        "CostCalculation");
                }

                _logger.LogInformation(
                    "[GET COST CALCULATION] Retrieved successfully. RequestId: {RequestId}",
                    request.RequestId);

                return ResponseViewModel<CostCalculationDto>.Success(dto);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[GET COST CALCULATION] Unexpected error. RequestId: {RequestId}",
                    request.RequestId);

                throw new BusinessLogicException(
                    "Failed to retrieve cost calculation",
                    ex,
                    "CostCalculation");
            }
        }
    }
}