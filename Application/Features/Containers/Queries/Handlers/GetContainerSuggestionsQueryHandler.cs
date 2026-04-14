// Application/Features/Containers/Queries/Handlers/GetContainerSuggestionsQueryHandler.cs
using Application.DTOs.ContainerDtos;
using Application.Features.Containers.Queries;
using Application.ViewModel;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Containers.Queries.Handlers
{
    public class GetContainerSuggestionsQueryHandler
        : IRequestHandler<GetContainerSuggestionsQuery, ResponseViewModel<List<ContainerSuggestionDto>>>
    {
        private readonly IGeneralRepository<Domain.Models.ImportRequest> _requestRepo;
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly ILogger<GetContainerSuggestionsQueryHandler> _logger;

        // Port → city mapping for location scoring
        private static readonly Dictionary<string, string[]> _portCityMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Alexandria"] = new[] { "Alexandria", "Cairo", "Giza", "Delta" },
            ["Port Said"] = new[] { "Port Said", "Ismailia", "Suez", "Cairo" },
            ["Damietta"] = new[] { "Damietta", "Mansoura", "Kafr El Sheikh" },
            ["Sokhna"] = new[] { "Cairo", "Suez", "Ain Sokhna" },
            ["Aqaba"] = new[] { "Aqaba", "Amman" },
            ["Jeddah"] = new[] { "Jeddah", "Mecca", "Riyadh" },
        };

        public GetContainerSuggestionsQueryHandler(
            IGeneralRepository<Domain.Models.ImportRequest> requestRepo,
            IGeneralRepository<Domain.Models.Container> containerRepo,
            ILogger<GetContainerSuggestionsQueryHandler> logger)
        {
            _requestRepo = requestRepo;
            _containerRepo = containerRepo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<ContainerSuggestionDto>>> Handle(
            GetContainerSuggestionsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[CONTAINER SUGGESTIONS] RequestId: {Id}", request.ImportRequestId);

            var importRequest = await _requestRepo.GetByIdAsync(request.ImportRequestId);
            if (importRequest is null)
                throw new NotFoundException("Import request not found.", "Container");

            // Only LCL requests need container suggestions
            if (importRequest.ShipmentType != ShipmentType.LCL)
                return ResponseViewModel<List<ContainerSuggestionDto>>.Success(
                    new List<ContainerSuggestionDto>(),
                    "FCL requests use a dedicated container.");

            var candidates = await _containerRepo.GetAll()
                .Where(c =>
                    c.Status == ContainerStatus.Open &&
                    c.ShipmentType == ShipmentType.LCL &&
                    c.CurrentWeightKg + importRequest.TotalWeightKg <= c.MaxWeightKg &&
                    c.CurrentVolumeCbm + importRequest.TotalVolumeCbm <= c.MaxVolumeCbm)
                .Include(c => c.Items)
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            if (!candidates.Any())
                return ResponseViewModel<List<ContainerSuggestionDto>>.Success(
                    new List<ContainerSuggestionDto>(),
                    "No suitable containers found.");

            var suggestions = candidates
                .Select(c => Score(c, importRequest))
                .OrderByDescending(s => s.Score)
                .ToList();

            if (suggestions.Any())
                suggestions[0].IsBestMatch = true;

            return ResponseViewModel<List<ContainerSuggestionDto>>.Success(suggestions);
        }

        private ContainerSuggestionDto Score(Domain.Models.Container c, Domain.Models.ImportRequest req)
        {
            var remainingWeightAfter = c.MaxWeightKg - (c.CurrentWeightKg + req.TotalWeightKg);
            var remainingVolumeAfter = c.MaxVolumeCbm - (c.CurrentVolumeCbm + req.TotalVolumeCbm);
            var weightUtilAfter = (c.CurrentWeightKg + req.TotalWeightKg) / c.MaxWeightKg * 100;
            var volumeUtilAfter = (c.CurrentVolumeCbm + req.TotalVolumeCbm) / c.MaxVolumeCbm * 100;

            // ── Space score (0–70): closer to 100% utilization = higher score
            var weightFillScore = (decimal)70 * (1 - remainingWeightAfter / c.MaxWeightKg);
            var volumeFillScore = (decimal)70 * (1 - remainingVolumeAfter / c.MaxVolumeCbm);
            var spaceScore = (weightFillScore + volumeFillScore) / 2;

            // ── Location score (0–30)
            var locationScore = ComputeLocationScore(c.DestinationPort, req.ShippingAddress);

            var total = (int)Math.Round(spaceScore + locationScore);
            total = Math.Clamp(total, 0, 100);

            return new ContainerSuggestionDto
            {
                ContainerId = c.Id,
                ContainerNumber = c.ContainerNumber,
                AvailableWeightKg = c.MaxWeightKg - c.CurrentWeightKg,
                AvailableVolumeCbm = c.MaxVolumeCbm - c.CurrentVolumeCbm,
                CurrentWeightKg = c.CurrentWeightKg,
                CurrentVolumeCbm = c.CurrentVolumeCbm,
                MaxWeightKg = c.MaxWeightKg,
                MaxVolumeCbm = c.MaxVolumeCbm,
                DestinationPort = c.DestinationPort,
                OriginPort = c.OriginPort,
                ItemCount = c.Items.Count(x => !x.IsDeleted),
                TotalShippingCost = c.TotalShippingCost,
                Score = total,
                WeightUtilizationAfter = Math.Round(weightUtilAfter, 1),
                VolumeUtilizationAfter = Math.Round(volumeUtilAfter, 1),
            };
        }

        private static decimal ComputeLocationScore(string? destinationPort, string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(destinationPort) || string.IsNullOrWhiteSpace(shippingAddress))
                return 0;

            foreach (var (port, cities) in _portCityMap)
            {
                if (!port.Equals(destinationPort, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var city in cities)
                {
                    if (shippingAddress.Contains(city, StringComparison.OrdinalIgnoreCase))
                        return 30;
                }
                // Port matches but city doesn't — partial credit
                return 10;
            }
            return 0;
        }
    }
}