using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Features.Containers.Queries.Handlers
{
    public class GetOfficeContainersQueryHandler
        : IRequestHandler<GetOfficeContainersQuery, ResponseViewModel<PaginatedResult<ContainerListItemDto>>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOfficeContainersQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetOfficeContainersQueryHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            IMapper mapper,
            ILogger<GetOfficeContainersQueryHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseViewModel<PaginatedResult<ContainerListItemDto>>> Handle(
            GetOfficeContainersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[GET OFFICE CONTAINERS] Fetching office containers. Page: {Page}, PageSize: {PageSize}, Status: {Status}",
                    request.Page, request.PageSize, request.Status);

                var userId = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var officeId))
                {
                    _logger.LogError("[GET OFFICE CONTAINERS] Unable to extract user ID from claims");
                    throw new BusinessLogicException(
                        "Unable to identify current office",
                        "Container",
                        AppErrorCode.UnauthorizedAccess);
                }

                var query = _containerRepo.GetAll()
                    .Where(x => x.ManagedByOfficeId == officeId);

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
                    "[GET OFFICE CONTAINERS] Retrieved {Count} containers. OfficeId: {OfficeId}, TotalCount: {TotalCount}",
                    containerDtos.Count, officeId, totalCount);

                return ResponseViewModel<PaginatedResult<ContainerListItemDto>>.Success(result);
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET OFFICE CONTAINERS] Unexpected error while retrieving office containers");
                throw new BusinessLogicException("Failed to retrieve containers", ex, "Container");
            }
        }
    }
}