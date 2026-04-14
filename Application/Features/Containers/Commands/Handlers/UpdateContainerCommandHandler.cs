using Application.DTOs.ContainerDtos;
using Application.Features.Containers.Commands;
using Application.ViewModel;
using AutoMapper;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands.Handlers
{
    public class UpdateContainerCommandHandler : IRequestHandler<UpdateContainerCommand, ResponseViewModel<ContainerDto>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateContainerCommandHandler> _logger;

        public UpdateContainerCommandHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            IMapper mapper,
            ILogger<UpdateContainerCommandHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<ContainerDto>> Handle(
            UpdateContainerCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE CONTAINER] Starting container update. ContainerId: {ContainerId}",
                    request.Id);

                // ─────────────────────────────────────────────────────────────────────────
                // GET CONTAINER
                // ─────────────────────────────────────────────────────────────────────────
                var container = await _containerRepo.GetByIdAsync(request.Id);
                if (container == null)
                {
                    _logger.LogWarning(
                        "[UPDATE CONTAINER] Container not found. ContainerId: {ContainerId}",
                        request.Id);

                    throw new NotFoundException(
                        $"Container with ID {request.Id} not found",
                        "Container");
                }

                // ─────────────────────────────────────────────────────────────────────────
                // CHECK IF CONTAINER CAN BE UPDATED
                // ─────────────────────────────────────────────────────────────────────────
                if (container.Status != ContainerStatus.Open)
                {
                    _logger.LogWarning(
                        "[UPDATE CONTAINER] Cannot update non-open container. ContainerId: {ContainerId}, Status: {Status}",
                        request.Id,
                        container.Status);

                    throw new BusinessLogicException(
                        "Container can only be updated while in Open status",
                        "Container");
                }

                // ─────────────────────────────────────────────────────────────────────────
                // UPDATE PROPERTIES
                // ─────────────────────────────────────────────────────────────────────────
                if (!string.IsNullOrWhiteSpace(request.ContainerNumber))
                    container.ContainerNumber = request.ContainerNumber.Trim();

                if (request.MaxWeightKg.HasValue && request.MaxWeightKg > 0)
                    container.MaxWeightKg = request.MaxWeightKg.Value;

                if (request.MaxVolumeCbm.HasValue && request.MaxVolumeCbm > 0)
                    container.MaxVolumeCbm = request.MaxVolumeCbm.Value;

                if (!string.IsNullOrWhiteSpace(request.OriginPort))
                    container.OriginPort = request.OriginPort.Trim();

                if (!string.IsNullOrWhiteSpace(request.DestinationPort))
                    container.DestinationPort = request.DestinationPort.Trim();

                if (request.ExpectedArrival.HasValue)
                    container.ExpectedArrival = request.ExpectedArrival;

                container.UpdatedAt = DateTime.UtcNow;

                await _containerRepo.SaveAsync();

                _logger.LogInformation(
                    "[UPDATE CONTAINER] Container updated successfully. ContainerId: {ContainerId}",
                    container.Id);

                var containerDto = _mapper.Map<ContainerDto>(container);
                return ResponseViewModel<ContainerDto>.Success(
                    containerDto,
                    "Container updated successfully");
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
                    "[UPDATE CONTAINER] Unexpected error while updating container. ContainerId: {ContainerId}",
                    request.Id);

                throw new BusinessLogicException(
                    "Failed to update container",
                    ex,
                    "Container");
            }
        }
    }
}

