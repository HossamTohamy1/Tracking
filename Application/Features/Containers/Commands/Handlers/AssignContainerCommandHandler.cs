// Application/Features/Containers/Commands/Handlers/AssignContainerCommandHandler.cs
using Application.ViewModel;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Containers.Commands.Handlers
{
    public class AssignContainerCommandHandler
        : IRequestHandler<AssignContainerCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IGeneralRepository<Container> _containerRepo;
        private readonly IGeneralRepository<ContainerItem> _containerItemRepo;
        private readonly ILogger<AssignContainerCommandHandler> _logger;

        public AssignContainerCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IGeneralRepository<Container> containerRepo,
            IGeneralRepository<ContainerItem> containerItemRepo,
            ILogger<AssignContainerCommandHandler> logger)
        {
            _requestRepo = requestRepo;
            _containerRepo = containerRepo;
            _containerItemRepo = containerItemRepo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            AssignContainerCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[ASSIGN CONTAINER] RequestId: {RId}, ContainerId: {CId}",
                command.ImportRequestId, command.ContainerId);

            var importRequest = await _requestRepo.GetByIdAsync(command.ImportRequestId);
            if (importRequest is null)
                throw new NotFoundException("Import request not found.", "Container");

            if (importRequest.Status != RequestStatus.Approved)
                throw new BusinessLogicException(
                    "Container can only be assigned to approved requests.", "Container");

            if (importRequest.ContainerItem is not null)
                throw new BusinessLogicException(
                    "This request is already assigned to a container.", "Container");

            var container = await _containerRepo.GetByIdAsync(command.ContainerId);
            if (container is null)
                throw new NotFoundException("Container not found.", "Container");

            if (container.Status != ContainerStatus.Open)
                throw new BusinessLogicException(
                    "Container must be Open to accept shipments.", "Container");

            if (container.ShipmentType != ShipmentType.LCL)
                throw new BusinessLogicException(
                    "Only LCL containers can be manually assigned.", "Container");

            if (container.CurrentWeightKg + importRequest.TotalWeightKg > container.MaxWeightKg)
                throw new BusinessLogicException(
                    "Shipment exceeds container weight capacity.", "Container");

            if (container.CurrentVolumeCbm + importRequest.TotalVolumeCbm > container.MaxVolumeCbm)
                throw new BusinessLogicException(
                    "Shipment exceeds container volume capacity.", "Container");

            var item = new ContainerItem
            {
                ContainerId = container.Id,
                ImportRequestId = importRequest.Id,
                WeightKg = importRequest.TotalWeightKg,
                VolumeCbm = importRequest.TotalVolumeCbm,
                CostShare = 0,
            };
            await _containerItemRepo.AddAsync(item);

            container.CurrentWeightKg += importRequest.TotalWeightKg;
            container.CurrentVolumeCbm += importRequest.TotalVolumeCbm;
            container.UpdatedAt = DateTime.UtcNow;

            // Recalculate cost shares
            if (container.TotalShippingCost > 0 && container.CurrentVolumeCbm > 0)
            {
                foreach (var existing in container.Items.Where(x => !x.IsDeleted))
                    existing.CostShare = (existing.VolumeCbm / container.CurrentVolumeCbm)
                                         * container.TotalShippingCost;
            }

            await _containerRepo.SaveAsync();

            _logger.LogInformation(
                "[ASSIGN CONTAINER] Done. RequestId: {RId} → ContainerId: {CId}",
                command.ImportRequestId, command.ContainerId);

            return ResponseViewModel<bool>.Success(true, "Container assigned successfully.");
        }
    }
}