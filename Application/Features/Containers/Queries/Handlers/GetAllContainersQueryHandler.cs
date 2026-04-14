using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Containers.Queries.Handlers
{
    public class GetAllContainersQueryHandler
        : IRequestHandler<GetAllContainersQuery, ResponseViewModel<PaginatedResult<ContainerListItemDto>>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllContainersQueryHandler> _logger;

        public GetAllContainersQueryHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            IMapper mapper,
            ILogger<GetAllContainersQueryHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<PaginatedResult<ContainerListItemDto>>> Handle(
            GetAllContainersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[GET ALL CONTAINERS] Global container search. Page: {Page}, PageSize: {PageSize}",
                    request.Page, request.PageSize);

                var query = _containerRepo.GetAll();

                if (request.Status.HasValue)
                    query = query.Where(x => (int)x.Status == request.Status.Value);

                var totalCount = await query.CountAsync(cancellationToken);

                var containers = await query
                    .Include(x => x.Items.Where(i => !i.IsDeleted))
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var containerDtos = _mapper.Map<List<ContainerListItemDto>>(containers);

                var result = new PaginatedResult<ContainerListItemDto>
                {
                    Items = containerDtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                _logger.LogInformation(
                    "[GET ALL CONTAINERS] Retrieved {Count} containers. TotalCount: {TotalCount}",
                    containerDtos.Count, totalCount);

                return ResponseViewModel<PaginatedResult<ContainerListItemDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET ALL CONTAINERS] Unexpected error while retrieving all containers");
                throw new BusinessLogicException("Failed to retrieve containers", ex, "Container");
            }
        }
    }
}