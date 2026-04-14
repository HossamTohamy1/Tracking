using Application.DTOs.ImportRequests;
using Application.ViewModel;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Application.Features.ImportRequests.Commands.UpdateStage
{
    public record UpdateStageCommand(Guid RequestId, UpdateStageDto Dto)
        : IRequest<ResponseViewModel<bool>>;

    public class UpdateStageCommandHandler
        : IRequestHandler<UpdateStageCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IGeneralRepository<Tracking> _trackingRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UpdateStageCommandHandler> _logger;

        private static readonly Dictionary<string, (RequestStatus Status, ShipmentStage Stage)> _allowedStages = new()
        {
            ["Processing"] = (RequestStatus.Processing, ShipmentStage.Processing),
            ["Shipped"] = (RequestStatus.Shipped, ShipmentStage.Shipped),
            ["Customs"] = (RequestStatus.Customs, ShipmentStage.Customs),
            ["OutForDelivery"] = (RequestStatus.OutForDelivery, ShipmentStage.OutForDelivery),
            ["Delivered"] = (RequestStatus.Delivered, ShipmentStage.Delivered),
        };

        private static readonly Dictionary<RequestStatus, string[]> _validTransitions = new()
        {
            [RequestStatus.Approved] = ["Processing"],
            [RequestStatus.Processing] = ["Shipped"],
            [RequestStatus.Shipped] = ["Customs"],
            [RequestStatus.Customs] = ["OutForDelivery"],
            [RequestStatus.OutForDelivery] = ["Delivered"],
        };

        public UpdateStageCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IGeneralRepository<Tracking> trackingRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UpdateStageCommandHandler> logger)
        {
            _requestRepo = requestRepo;
            _trackingRepo = trackingRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            UpdateStageCommand command,
            CancellationToken cancellationToken)
        {
            var officeId = Guid.Parse(
                _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _logger.LogInformation(
                "Office {OfficeId} is updating stage of ImportRequest {RequestId} to {Stage}",
                officeId, command.RequestId, command.Dto.Stage);

            // ── 1. Validate stage string ──────────────────────────────────
            if (!_allowedStages.TryGetValue(command.Dto.Stage, out var target))
                throw new Domain.Exceptions.ValidationException(
                    $"Invalid stage '{command.Dto.Stage}'.",
                    new Dictionary<string, string[]>
                    {
                        ["Stage"] = [$"Allowed values: {string.Join(", ", _allowedStages.Keys)}"]
                    },
                    "ImportRequests");

            // ── 2. Load request ───────────────────────────────────────────
            var importRequest = await _requestRepo.GetByIdAsync(command.RequestId);
            if (importRequest is null)
                throw new NotFoundException("Import request not found.", "ImportRequests");

            // ── 3. Authorization: only the assigned office ────────────────
            if (importRequest.AssignedOfficeId != officeId)
                throw new UnauthorizedAccessException(
                    "You are not the assigned office for this request.");

            // ── 4. Transition guard (must follow the pipeline order) ──────
            if (!_validTransitions.TryGetValue(importRequest.Status, out var allowed)
                || !allowed.Contains(command.Dto.Stage))
            {
                throw new BusinessLogicException(
                    $"Cannot move to '{command.Dto.Stage}' from '{importRequest.Status}'. " +
                    $"Expected next stage(s): {string.Join(", ", allowed ?? [])}",
                    "ImportRequests");
            }

            // ── 5. Update ImportRequest.Status ────────────────────────────
            await _requestRepo.UpdatePartialAsync(
                new ImportRequest
                {
                    Id = importRequest.Id,
                    Status = target.Status,
                    UpdatedAt = DateTime.UtcNow
                },
                nameof(ImportRequest.Status),
                nameof(ImportRequest.UpdatedAt));

            // ── 6. Update Tracking record ─────────────────────────────────
            var tracking = await _trackingRepo
                .GetAll()
                .FirstOrDefaultAsync(t => t.ImportRequestId == importRequest.Id, cancellationToken);

            if (tracking is not null)
            {
                var updatedTracking = new Tracking
                {
                    Id = tracking.Id,
                    CurrentStage = target.Stage,
                    UpdatedAt = DateTime.UtcNow,
                };

                var modifiedProps = new List<string>
                {
                    nameof(Tracking.CurrentStage),
                    nameof(Tracking.UpdatedAt),
                };

                // الـ optional fields
                if (command.Dto.Location is not null)
                {
                    updatedTracking.CurrentLocation = command.Dto.Location;
                    modifiedProps.Add(nameof(Tracking.CurrentLocation));
                }
                if (command.Dto.TrackingNumber is not null)
                {
                    updatedTracking.TrackingNumber = command.Dto.TrackingNumber;
                    modifiedProps.Add(nameof(Tracking.TrackingNumber));
                }
                if (command.Dto.CarrierName is not null)
                {
                    updatedTracking.CarrierName = command.Dto.CarrierName;
                    modifiedProps.Add(nameof(Tracking.CarrierName));
                }
                if (command.Dto.EstimatedDeliveryDate is not null)
                {
                    updatedTracking.EstimatedDeliveryDate = command.Dto.EstimatedDeliveryDate;
                    modifiedProps.Add(nameof(Tracking.EstimatedDeliveryDate));
                }

                if (target.Stage == ShipmentStage.Shipped)
                {
                    updatedTracking.ShippedAt = DateTime.UtcNow;
                    modifiedProps.Add(nameof(Tracking.ShippedAt));
                }
                else if (target.Stage == ShipmentStage.Customs)
                {
                    updatedTracking.ArrivedPortAt = DateTime.UtcNow;
                    modifiedProps.Add(nameof(Tracking.ArrivedPortAt));
                }
                else if (target.Stage == ShipmentStage.Delivered)
                {
                    updatedTracking.DeliveredAt = DateTime.UtcNow;
                    modifiedProps.Add(nameof(Tracking.DeliveredAt));
                }

                await _trackingRepo.UpdatePartialAsync(updatedTracking, [.. modifiedProps]);

               
            }

            _logger.LogInformation(
                "ImportRequest {RequestId} moved to stage {Stage} by Office {OfficeId}",
                command.RequestId, command.Dto.Stage, officeId);

            return ResponseViewModel<bool>.Success(
                true,
                $"Request moved to '{command.Dto.Stage}' successfully.");
        }
    }
}