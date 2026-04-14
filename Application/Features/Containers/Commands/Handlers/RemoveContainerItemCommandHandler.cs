using Application.DTOs.ContainerDtos;
using Application.Features.Containers.Commands;
using Application.ViewModel;
using AutoMapper;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Containers.Commands.Handlers
{
    public class RemoveContainerItemCommandHandler : IRequestHandler<RemoveContainerItemCommand, ResponseViewModel<ContainerDto>>
    {
        private readonly IGeneralRepository<Container> _containerRepo;
        private readonly IGeneralRepository<ContainerItem> _containerItemRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<RemoveContainerItemCommandHandler> _logger;

        public RemoveContainerItemCommandHandler(
            IGeneralRepository<Container> containerRepo,
            IGeneralRepository<ContainerItem> containerItemRepo,
            IMapper mapper,
            ILogger<RemoveContainerItemCommandHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _containerItemRepo = containerItemRepo ?? throw new ArgumentNullException(nameof(containerItemRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<ContainerDto>> Handle(
            RemoveContainerItemCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[REMOVE CONTAINER ITEM] ContainerId: {ContainerId}, ItemId: {ItemId}",
                    request.ContainerId, request.ItemId);

                var container = await _containerRepo.GetByIdAsync(request.ContainerId);
                if (container == null)
                    throw new NotFoundException($"Container with ID {request.ContainerId} not found", "Container");

                if (container.Status != ContainerStatus.Open)
                    throw new BusinessLogicException(
                        "Items can only be removed from open containers.",
                        "Container");

                var item = container.Items.FirstOrDefault(x => x.Id == request.ItemId && !x.IsDeleted);
                if (item == null)
                    throw new NotFoundException($"Item with ID {request.ItemId} not found in this container", "Container");

                container.CurrentWeightKg -= item.WeightKg;
                container.CurrentVolumeCbm -= item.VolumeCbm;
                container.UpdatedAt = DateTime.UtcNow;

                await _containerItemRepo.DeleteAsync(item.Id);


                var remainingItems = container.Items
                    .Where(x => x.Id != request.ItemId && !x.IsDeleted)
                    .ToList();

                if (remainingItems.Any() && container.CurrentVolumeCbm > 0)
                {
                    foreach (var remainingItem in remainingItems)
                    {
                        remainingItem.CostShare =
                            (remainingItem.VolumeCbm / container.CurrentVolumeCbm) * container.TotalShippingCost;
                    }
                }

                await _containerRepo.SaveAsync();

                _logger.LogInformation(
                    "[REMOVE CONTAINER ITEM] Item removed. ContainerId: {ContainerId}, ItemId: {ItemId}, RemainingItems: {Count}",
                    container.Id, request.ItemId, remainingItems.Count);

                var containerDto = _mapper.Map<ContainerDto>(container);
                return ResponseViewModel<ContainerDto>.Success(containerDto, "Item removed from container successfully");
            }
            catch (NotFoundException) { throw; }
            catch (BusinessLogicException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[REMOVE CONTAINER ITEM] Unexpected error. ContainerId: {ContainerId}, ItemId: {ItemId}",
                    request.ContainerId, request.ItemId);
                throw new BusinessLogicException("Failed to remove item from container", ex, "Container");
            }
        }
    }
}