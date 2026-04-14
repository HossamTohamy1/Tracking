using Application.DTOs.ContainerDtos;
using Application.Features.Containers.Commands;
using Application.ViewModel;
using AutoMapper;
using Domain.Enums;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Application.Features.Containers.Commands.Handlers
{
    public class CreateContainerCommandHandler : IRequestHandler<CreateContainerCommand, ResponseViewModel<ContainerDto>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateContainerCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateContainerCommandHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            IMapper mapper,
            ILogger<CreateContainerCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseViewModel<ContainerDto>> Handle(
            CreateContainerCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ── Validation ────────────────────────────────────────────
                var validationErrors = ValidateCreateRequest(request);
                if (validationErrors.Any())
                    throw new ValidationException("Container creation validation failed", validationErrors, "Container");

                // ── Get Office ID from Claims ──────────────────────────────
                var userId = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var officeId))
                    throw new BusinessLogicException(
                        "Unable to identify current office",
                        "Container",
                        AppErrorCode.UnauthorizedAccess);

                // ── Create Container ──────────────────────────────────────
                var container = new Domain.Models.Container
                {
                    Id = Guid.NewGuid(),
                    ContainerNumber = GenerateContainerNumber(),
                    Status = ContainerStatus.Open,
                    MaxWeightKg = request.MaxWeightKg,
                    MaxVolumeCbm = request.MaxVolumeCbm,
                    CurrentWeightKg = 0,
                    CurrentVolumeCbm = 0,
                    OriginPort = request.OriginPort?.Trim(),
                    DestinationPort = request.DestinationPort?.Trim(),
                    ExpectedArrival = request.ExpectedArrival,
                    ManagedByOfficeId = officeId,
                    TotalShippingCost = 0,
                    ShipmentType = request.ShipmentType,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _containerRepo.AddAsync(container);

                _logger.LogInformation(
                    "[CREATE CONTAINER] Container created. ContainerId: {ContainerId}, Number: {Number}, Type: {Type}",
                    container.Id, container.ContainerNumber, container.ShipmentType);

                var containerDto = _mapper.Map<ContainerDto>(container);
                return ResponseViewModel<ContainerDto>.Success(containerDto, "Container created successfully");
            }
            catch (ValidationException) { throw; }
            catch (BusinessLogicException) { throw; }
            catch (Exception ex)
            {
                throw new BusinessLogicException("Failed to create container", ex, "Container");
            }
        }

        private Dictionary<string, string[]> ValidateCreateRequest(CreateContainerCommand request)
        {
            var errors = new Dictionary<string, string[]>();

            if (request.MaxWeightKg <= 0)
                errors.Add("MaxWeightKg", new[] { "Max weight must be greater than 0" });

            if (request.MaxVolumeCbm <= 0)
                errors.Add("MaxVolumeCbm", new[] { "Max volume must be greater than 0" });

            return errors;
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
                    .Replace("CNT-", "")
                    .TrimStart('0');
                if (int.TryParse(lastNumber, out int parsed))
                    nextNumber = parsed + 1;
            }

            return $"CNT-{nextNumber:D6}";
        }
    }
}