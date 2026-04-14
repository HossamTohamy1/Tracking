// GetContainerByIdQueryHandler.cs  (FIXED)
// Problem: GetByIdAsync uses FirstOrDefaultAsync with no .Include() calls,
//          so container.Items is always empty when the modal opens.
// Fix:     Use IQueryable directly with .Include() before fetching.

using Application.DTOs.ContainerDtos;
using Application.Features.Containers.Queries;
using Application.ViewModel;
using AutoMapper;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Containers.Queries.Handlers
{
    public class GetContainerByIdQueryHandler
        : IRequestHandler<GetContainerByIdQuery, ResponseViewModel<ContainerDto>>
    {
        private readonly IGeneralRepository<Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetContainerByIdQueryHandler> _logger;

        public GetContainerByIdQueryHandler(
            IGeneralRepository<Container> containerRepo,
            IMapper mapper,
            ILogger<GetContainerByIdQueryHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<ContainerDto>> Handle(
            GetContainerByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[GET CONTAINER BY ID] Fetching container. ContainerId: {ContainerId}",
                request.ContainerId);

            // ── KEY FIX: use GetAll() so we can chain .Include() ─────────────
            var container = await _containerRepo
                .GetAll()
                .Where(c => c.Id == request.ContainerId)
                .Include(c => c.ManagedByOffice)               // for ManagedByOfficeName
                .Include(c => c.Items.Where(i => !i.IsDeleted)) // ← items were missing!
                    .ThenInclude(i => i.ImportRequest)           // for RequestNumber
                .FirstOrDefaultAsync(cancellationToken);

            if (container is null)
            {
                _logger.LogWarning(
                    "[GET CONTAINER BY ID] Container not found. ContainerId: {ContainerId}",
                    request.ContainerId);

                throw new NotFoundException(
                    $"Container with ID {request.ContainerId} not found",
                    "Container");
            }

            var containerDto = _mapper.Map<ContainerDto>(container);

            _logger.LogInformation(
                "[GET CONTAINER BY ID] Container retrieved. ContainerId: {ContainerId}, Items: {Count}",
                request.ContainerId, container.Items.Count);

            return ResponseViewModel<ContainerDto>.Success(containerDto);
        }
    }
}