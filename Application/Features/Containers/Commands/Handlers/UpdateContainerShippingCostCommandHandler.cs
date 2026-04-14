using Application.DTOs.ContainerDtos;
using Application.Features.Containers.Commands;
using Application.ViewModel;
using AutoMapper;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands.Handlers
{
    public class UpdateContainerShippingCostCommandHandler : IRequestHandler<UpdateContainerShippingCostCommand, ResponseViewModel<ContainerDto>>
    {
        private readonly IGeneralRepository<Container> _containerRepo;
        private readonly IGeneralRepository<ContainerItem> _containerItemRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateContainerShippingCostCommandHandler> _logger;

        public UpdateContainerShippingCostCommandHandler(
            IGeneralRepository<Container> containerRepo,
            IGeneralRepository<ContainerItem> containerItemRepo,
            IMapper mapper,
            ILogger<UpdateContainerShippingCostCommandHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _containerItemRepo = containerItemRepo ?? throw new ArgumentNullException(nameof(containerItemRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<ContainerDto>> Handle(
            UpdateContainerShippingCostCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE SHIPPING COST] Setting shipping cost. ContainerId: {ContainerId}, Cost: {Cost}",
                    request.ContainerId,
                    request.TotalShippingCost);

                var container = await _containerRepo.GetByIdAsync(request.ContainerId);
                if (container == null)
                {
                    _logger.LogWarning(
                        "[UPDATE SHIPPING COST] Container not found. ContainerId: {ContainerId}",
                        request.ContainerId);

                    throw new NotFoundException(
                        $"Container with ID {request.ContainerId} not found",
                        "Container");
                }

                if (request.TotalShippingCost < 0)
                {
                    _logger.LogWarning(
                        "[UPDATE SHIPPING COST] Invalid shipping cost. Cost: {Cost}",
                        request.TotalShippingCost);

                    throw new BusinessLogicException(
                        "Shipping cost cannot be negative",
                        "Container");
                }

                container.TotalShippingCost = request.TotalShippingCost;
                container.UpdatedAt = DateTime.UtcNow;

                // ─────────────────────────────────────────────────────────────────────────
                // RECALCULATE COST SHARES FOR ALL ITEMS
                // ─────────────────────────────────────────────────────────────────────────
                if (container.Items.Any() && container.CurrentVolumeCbm > 0)
                {
                    foreach (var item in container.Items)
                    {
                        item.CostShare = (item.VolumeCbm / container.CurrentVolumeCbm) * request.TotalShippingCost;
                    }
                }

                await _containerRepo.SaveAsync();

                _logger.LogInformation(
                    "[UPDATE SHIPPING COST] Shipping cost updated and cost shares recalculated. ContainerId: {ContainerId}",
                    container.Id);

                var containerDto = _mapper.Map<ContainerDto>(container);
                return ResponseViewModel<ContainerDto>.Success(
                    containerDto,
                    "Shipping cost updated successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[UPDATE SHIPPING COST] Unexpected error. ContainerId: {ContainerId}",
                    request.ContainerId);

                throw new BusinessLogicException(
                    "Failed to update shipping cost",
                    ex,
                    "Container");
            }
        }
    }
}
 