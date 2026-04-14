using Application.DTOs.CostCalculationDtos;
using Application.Features.Containers.Queries;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Application.Features.CostCalculations.Queries.Handlers
{
    public class GetMyCostCalculationsQueryHandler
        : IRequestHandler<GetMyCostCalculationsQuery, ResponseViewModel<PaginatedResult<CostCalculationDto>>>
    {
        private readonly IGeneralRepository<CostCalculation> _costRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetMyCostCalculationsQueryHandler> _logger;

        public GetMyCostCalculationsQueryHandler(
            IGeneralRepository<CostCalculation> costRepo,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GetMyCostCalculationsQueryHandler> logger)
        {
            _costRepo = costRepo ?? throw new ArgumentNullException(nameof(costRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<PaginatedResult<CostCalculationDto>>> Handle(
            GetMyCostCalculationsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ── استخراج الـ CustomerId من الـ JWT token ────────────────
                var userIdStr = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var customerId))
                {
                    _logger.LogWarning("[GET MY COST CALCULATIONS] Unable to extract user ID from claims.");
                    throw new BusinessLogicException(
                        "Unable to identify current user.",
                        "CostCalculation");
                }

                _logger.LogInformation(
                    "[GET MY COST CALCULATIONS] CustomerId: {CustomerId}, Page: {Page}, PageSize: {PageSize}",
                    customerId, request.Page, request.PageSize);

                // ── فلترة: بس الطلبات اللي UserId = customerId ─────────────
                // هذا هو الـ Security Guard — Customer لا يشوف غير بياناته
                var query = _costRepo.GetAll()
                    .Where(c => c.ImportRequest.UserId == customerId);

                if (!string.IsNullOrWhiteSpace(request.Currency))
                    query = query.Where(c => c.Currency == request.Currency.Trim().ToUpper());

                // ── Pagination ────────────────────────────────────────────
                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ProjectTo<CostCalculationDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                var result = new PaginatedResult<CostCalculationDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                _logger.LogInformation(
                    "[GET MY COST CALCULATIONS] Retrieved {Count} records for CustomerId: {CustomerId}",
                    items.Count, customerId);

                return ResponseViewModel<PaginatedResult<CostCalculationDto>>.Success(result);
            }
            catch (BusinessLogicException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET MY COST CALCULATIONS] Unexpected error.");
                throw new BusinessLogicException(
                    "Failed to retrieve your cost calculations.",
                    ex,
                    "CostCalculation");
            }
        }
    }
}