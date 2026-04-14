using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using AutoMapper;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Queries.Handlers
{
    public class GetContainerCostBreakdownQueryHandler : IRequestHandler<GetContainerCostBreakdownQuery, ResponseViewModel<ContainerCostBreakdownDto>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetContainerCostBreakdownQueryHandler> _logger;

        public GetContainerCostBreakdownQueryHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            IMapper mapper,
            ILogger<GetContainerCostBreakdownQueryHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<ContainerCostBreakdownDto>> Handle(
            GetContainerCostBreakdownQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[GET COST BREAKDOWN] Calculating cost breakdown. ContainerId: {ContainerId}",
                    request.ContainerId);

                var container = await _containerRepo.GetByIdAsync(request.ContainerId);
                if (container == null)
                {
                    _logger.LogWarning(
                        "[GET COST BREAKDOWN] Container not found. ContainerId: {ContainerId}",
                        request.ContainerId);

                    throw new NotFoundException(
                        $"Container with ID {request.ContainerId} not found",
                        "Container");
                }

                // ─────────────────────────────────────────────────────────────────────────
                // BUILD COST BREAKDOWN
                // ─────────────────────────────────────────────────────────────────────────
                var breakdown = new ContainerCostBreakdownDto
                {
                    ContainerId = container.Id,
                    ContainerNumber = container.ContainerNumber,
                    TotalShippingCost = container.TotalShippingCost,
                    TotalVolumeCbm = container.CurrentVolumeCbm,
                    Items = new List<CostShareItemDto>()
                };

                // ─────────────────────────────────────────────────────────────────────────
                // CALCULATE COST SHARES PER ITEM
                // ─────────────────────────────────────────────────────────────────────────
                if (container.Items.Any() && container.CurrentVolumeCbm > 0)
                {
                    foreach (var item in container.Items.Where(x => !x.IsDeleted))
                    {
                        var volumePercentage = (item.VolumeCbm / container.CurrentVolumeCbm) * 100;
                        var costShare = (item.VolumeCbm / container.CurrentVolumeCbm) * container.TotalShippingCost;

                        breakdown.Items.Add(new CostShareItemDto
                        {
                            ContainerItemId = item.Id,
                            ImportRequestId = item.ImportRequestId,
                            RequestNumber = item.ImportRequest.Id.ToString().Substring(0, 8),
                            VolumeCbm = item.VolumeCbm,
                            WeightKg = item.WeightKg,
                            VolumePercentage = volumePercentage,
                            CostShare = costShare,
                            CustomerName = item.ImportRequest.User.FullName
                        });
                    }
                }

                _logger.LogInformation(
                    "[GET COST BREAKDOWN] Cost breakdown calculated. ContainerId: {ContainerId}, ItemCount: {ItemCount}",
                    request.ContainerId,
                    breakdown.Items.Count);

                return ResponseViewModel<ContainerCostBreakdownDto>.Success(breakdown);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[GET COST BREAKDOWN] Unexpected error. ContainerId: {ContainerId}",
                    request.ContainerId);

                throw new BusinessLogicException(
                    "Failed to calculate cost breakdown",
                    ex,
                    "Container");
            }
        }
    }
}