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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Features.ImportRequests.Commands.Approve
{
    public record ApproveImportRequestCommand(Guid RequestId, ApproveImportRequestDto Dto) : IRequest<ResponseViewModel<bool>>;

    public class ApproveImportRequestCommandHandler
        : IRequestHandler<ApproveImportRequestCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IGeneralRepository<Tracking> _trackingRepo;
        private readonly IGeneralRepository<CostCalculation> _costRepo;
        private readonly IGeneralRepository<Product> _productRepo;
        private readonly IGeneralRepository<Container> _containerRepo;
        private readonly IGeneralRepository<ContainerItem> _containerItemRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApproveImportRequestCommandHandler> _logger;

        public ApproveImportRequestCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IGeneralRepository<Tracking> trackingRepo,
            IGeneralRepository<CostCalculation> costRepo,
            IGeneralRepository<Product> productRepo,
            IGeneralRepository<Container> containerRepo,
            IGeneralRepository<ContainerItem> containerItemRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApproveImportRequestCommandHandler> logger)
        {
            _requestRepo = requestRepo;
            _trackingRepo = trackingRepo;
            _costRepo = costRepo;
            _productRepo = productRepo;
            _containerRepo = containerRepo;
            _containerItemRepo = containerItemRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            ApproveImportRequestCommand command,
            CancellationToken cancellationToken)
        {
            var officeId = Guid.Parse(
                _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _logger.LogInformation("Office {OfficeId} is approving ImportRequest {RequestId}",
                officeId, command.RequestId);

            // ── Load Import Request ────────────────────────────────────────
            var importRequest = await _requestRepo.GetByIdAsync(command.RequestId);
            if (importRequest is null)
                throw new NotFoundException("Import request not found.", "ImportRequests");

            if (importRequest.AssignedOfficeId != officeId)
                throw new UnauthorizedAccessException("You are not the assigned office for this request.");

            if (importRequest.Status != RequestStatus.Pending)
                throw new BusinessLogicException("Only pending requests can be approved.", "ImportRequests");

            // ── Load Product & Validate Stock ──────────────────────────────
            var product = await _productRepo.GetByIdAsync(importRequest.ProductId);
            if (product is null)
                throw new NotFoundException("Product not found.", "ImportRequests");

            var newStock = product.StockQuantity - importRequest.Quantity;
            if (newStock < 0)
                throw new BusinessLogicException(
                    $"Insufficient stock. Available: {product.StockQuantity}, Requested: {importRequest.Quantity}.",
                    "ImportRequests");

            // ── 1. Update Import Request Status ────────────────────────────
            await _requestRepo.UpdatePartialAsync(
                new ImportRequest { Id = importRequest.Id, Status = RequestStatus.Approved, UpdatedAt = DateTime.UtcNow },
                nameof(ImportRequest.Status), nameof(ImportRequest.UpdatedAt));

            // ── 2. Deduct Stock ────────────────────────────────────────────
            await _productRepo.UpdatePartialAsync(
                new Product { Id = product.Id, StockQuantity = newStock, UpdatedAt = DateTime.UtcNow },
                nameof(Product.StockQuantity), nameof(Product.UpdatedAt));

            _logger.LogInformation("Product {ProductId} stock reduced by {Qty}. New stock: {New}",
                product.Id, importRequest.Quantity, newStock);

            // ── 3. Auto-create Tracking ────────────────────────────────────
            var tracking = new Tracking
            {
                ImportRequestId = importRequest.Id,
                CurrentStage = ShipmentStage.Purchased
            };
            await _trackingRepo.AddAsync(tracking);

            // ── 4. Auto-create CostCalculation ────────────────────────────
            var costCalculation = new CostCalculation
            {
                ImportRequestId = importRequest.Id,
                WeightKg = importRequest.TotalWeightKg,
                VolumeCbm = importRequest.TotalVolumeCbm,
                BaseShippingCost = 0,
                CustomsDuty = 0,
                TaxAmount = 0,
                InsuranceCost = 0,
                HandlingFee = 0,
                OtherFees = 0,
                DiscountAmount = 0,
                TotalBeforeDiscount = 0,
                FinalAmount = 0,
                Currency = "USD"
            };
            await _costRepo.AddAsync(costCalculation);

            // ── 5. Route to Container ──────────────────────────────────────
            await RouteToContainerAsync(importRequest, officeId, command.Dto, cancellationToken);

            _logger.LogInformation(
                "ImportRequest {RequestId} approved. Tracking {TrackingId}, CostCalculation {CostId} created.",
                command.RequestId, tracking.Id, costCalculation.Id);

            return ResponseViewModel<bool>.Success(true,
                "Import request approved. Stock updated, Tracking, CostCalculation, and Container routing done.");
        }

        private async Task RouteToContainerAsync(
            ImportRequest importRequest,
            Guid officeId,
            ApproveImportRequestDto dto,
            CancellationToken cancellationToken)
        {
            if (importRequest.ShipmentType == ShipmentType.LCL)
            {
                _logger.LogInformation(
                    "[STAGE 5 - LCL] Skipping auto-assignment. " +
                    "Office will assign via container suggestions. RequestId: {Id}",
                    importRequest.Id);
            }
            else
            {
                _logger.LogInformation(
                    "[STAGE 5 - FCL] Creating dedicated container for request {RequestId}.",
                    importRequest.Id);

                var newContainer = new Container
                {
                    ContainerNumber = GenerateContainerNumber(),
                    Status = ContainerStatus.Open,
                    ShipmentType = ShipmentType.FullContainer,
                    MaxWeightKg = importRequest.TotalWeightKg,
                    MaxVolumeCbm = importRequest.TotalVolumeCbm,
                    CurrentWeightKg = 0,
                    CurrentVolumeCbm = 0,
                    ManagedByOfficeId = officeId,
                    TotalShippingCost = 0,

                    ExpectedArrival = dto.ExpectedArrival ?? importRequest.RequestedDeliveryDate,
                    OriginPort = dto.OriginPort,
                    DestinationPort = dto.DestinationPort,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _containerRepo.AddAsync(newContainer);
                await AddItemToContainerAsync(newContainer, importRequest);
            }
        }

        private async Task AddItemToContainerAsync(Container container, ImportRequest importRequest)
        {
            var containerItem = new ContainerItem
            {
                ContainerId = container.Id,
                ImportRequestId = importRequest.Id,
                WeightKg = importRequest.TotalWeightKg,
                VolumeCbm = importRequest.TotalVolumeCbm,
                CostShare = 0
            };
            await _containerItemRepo.AddAsync(containerItem);

            container.CurrentWeightKg += importRequest.TotalWeightKg;
            container.CurrentVolumeCbm += importRequest.TotalVolumeCbm;
            container.UpdatedAt = DateTime.UtcNow;

            if (container.TotalShippingCost > 0 && container.CurrentVolumeCbm > 0)
            {
                foreach (var item in container.Items.Where(x => !x.IsDeleted))
                {
                    item.CostShare = (item.VolumeCbm / container.CurrentVolumeCbm) * container.TotalShippingCost;
                }
            }

            await _containerRepo.SaveAsync();

            _logger.LogInformation(
                "[CONTAINER ITEM ADDED] Request {RequestId} added to Container {ContainerId}. " +
                "Container now: {Weight}kg / {Volume}cbm",
                importRequest.Id, container.Id,
                container.CurrentWeightKg, container.CurrentVolumeCbm);
        }

        private string GenerateContainerNumber()
        {
            var lastContainer = _containerRepo.GetAll()
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastContainer != null)
            {
                var lastNumber = lastContainer.ContainerNumber
                    .Replace("CNT-", "").TrimStart('0');
                if (int.TryParse(lastNumber, out int parsed))
                    nextNumber = parsed + 1;
            }

            return $"CNT-{nextNumber:D6}";
        }
    }
}