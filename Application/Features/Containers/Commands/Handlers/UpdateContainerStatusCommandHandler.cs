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

namespace Application.Features.Containers.Commands.Handlers
{
    public class UpdateContainerStatusCommandHandler : IRequestHandler<UpdateContainerStatusCommand, ResponseViewModel<ContainerDto>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateContainerStatusCommandHandler> _logger;

        private static readonly Dictionary<ContainerStatus, ContainerStatus> _validTransitions = new()
        {
            [ContainerStatus.Closed] = ContainerStatus.Shipped,
            [ContainerStatus.Shipped] = ContainerStatus.InTransit,
            [ContainerStatus.InTransit] = ContainerStatus.ArrivedPort,
            [ContainerStatus.ArrivedPort] = ContainerStatus.Customs,
            [ContainerStatus.Customs] = ContainerStatus.Delivered,
        };

        public UpdateContainerStatusCommandHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            IMapper mapper,
            ILogger<UpdateContainerStatusCommandHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<ContainerDto>> Handle(
            UpdateContainerStatusCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE CONTAINER STATUS] ContainerId: {ContainerId}, NewStatus: {Status}",
                    request.ContainerId, request.Status);

                var container = await _containerRepo.GetByIdAsync(request.ContainerId);
                if (container == null)
                    throw new NotFoundException($"Container with ID {request.ContainerId} not found", "Container");

                // ── Validate enum value ───────────────────────────────────
                if (!Enum.IsDefined(typeof(ContainerStatus), request.Status))
                    throw new BusinessLogicException("Invalid container status", "Container");

                var newStatus = (ContainerStatus)request.Status;
                var oldStatus = container.Status;

     
                if (newStatus == ContainerStatus.Open || newStatus == ContainerStatus.Cancelled)
                    throw new BusinessLogicException(
                        $"Cannot manually set status to '{newStatus}'. Use the dedicated endpoint.",
                        "Container");

                if (!_validTransitions.TryGetValue(oldStatus, out var expectedNext) || expectedNext != newStatus)
                    throw new BusinessLogicException(
                        $"Cannot move container from '{oldStatus}' to '{newStatus}'. " +
                        $"Expected next status: '{(_validTransitions.TryGetValue(oldStatus, out var next) ? next : "none")}'.",
                        "Container");

                // ── Apply Status ──────────────────────────────────────────
                container.Status = newStatus;
                container.UpdatedAt = DateTime.UtcNow;

                // ── Set Timestamps ────────────────────────────────────────
                if (newStatus == ContainerStatus.Shipped)
                    container.ShippedAt = DateTime.UtcNow;
                else if (newStatus == ContainerStatus.Delivered)
                    container.DeliveredAt = DateTime.UtcNow;

                await _containerRepo.SaveAsync();

                _logger.LogInformation(
                    "[UPDATE CONTAINER STATUS] Status updated. ContainerId: {ContainerId}, {Old} → {New}",
                    container.Id, oldStatus, newStatus);

                var containerDto = _mapper.Map<ContainerDto>(container);
                return ResponseViewModel<ContainerDto>.Success(containerDto, "Container status updated successfully");
            }
            catch (NotFoundException) { throw; }
            catch (BusinessLogicException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UPDATE CONTAINER STATUS] Unexpected error. ContainerId: {ContainerId}", request.ContainerId);
                throw new BusinessLogicException("Failed to update container status", ex, "Container");
            }
        }
    }
}