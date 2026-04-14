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
    public class CloseContainerCommandHandler : IRequestHandler<CloseContainerCommand, ResponseViewModel<ContainerDto>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CloseContainerCommandHandler> _logger;

        public CloseContainerCommandHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            IMapper mapper,
            ILogger<CloseContainerCommandHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<ContainerDto>> Handle(
            CloseContainerCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[CLOSE CONTAINER] Starting container closure. ContainerId: {ContainerId}",
                    request.ContainerId);

                var container = await _containerRepo.GetByIdAsync(request.ContainerId);
                if (container == null)
                {
                    _logger.LogWarning(
                        "[CLOSE CONTAINER] Container not found. ContainerId: {ContainerId}",
                        request.ContainerId);

                    throw new NotFoundException(
                        $"Container with ID {request.ContainerId} not found",
                        "Container");
                }

                if (container.Status != ContainerStatus.Open)
                {
                    _logger.LogWarning(
                        "[CLOSE CONTAINER] Cannot close non-open container. ContainerId: {ContainerId}, Status: {Status}",
                        request.ContainerId,
                        container.Status);

                    throw new BusinessLogicException(
                        "Only open containers can be closed",
                        "Container");
                }

                container.Status = ContainerStatus.Closed;
                container.UpdatedAt = DateTime.UtcNow;

                await _containerRepo.SaveAsync();

                _logger.LogInformation(
                    "[CLOSE CONTAINER] Container closed successfully. ContainerId: {ContainerId}",
                    container.Id);

                var containerDto = _mapper.Map<ContainerDto>(container);
                return ResponseViewModel<ContainerDto>.Success(
                    containerDto,
                    "Container closed successfully");
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
                    "[CLOSE CONTAINER] Unexpected error while closing container. ContainerId: {ContainerId}",
                    request.ContainerId);

                throw new BusinessLogicException(
                    "Failed to close container",
                    ex,
                    "Container");
            }
        }
    }
}
