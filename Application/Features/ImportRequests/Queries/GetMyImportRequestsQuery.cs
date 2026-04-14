using Application.DTOs.ImportRequests;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums.Enums_Models;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Text;

namespace Application.Features.ImportRequests.Queries
{
    public record GetMyImportRequestsQuery(string? Status) : IRequest<ResponseViewModel<List<ImportRequestListDto>>>;

    public class GetMyImportRequestsQueryHandler
        : IRequestHandler<GetMyImportRequestsQuery, ResponseViewModel<List<ImportRequestListDto>>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetMyImportRequestsQueryHandler> _logger;

        public GetMyImportRequestsQueryHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GetMyImportRequestsQueryHandler> logger)
        {
            _requestRepo = requestRepo;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<ImportRequestListDto>>> Handle(
            GetMyImportRequestsQuery query,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _logger.LogInformation("Customer {UserId} listing own import requests. Filter: {Status}", userId, query.Status);

            var dbQuery = _requestRepo.GetAll()
                .Where(r => r.UserId == userId);

            if (!string.IsNullOrWhiteSpace(query.Status)
                && Enum.TryParse<RequestStatus>(query.Status, ignoreCase: true, out var parsedStatus))
            {
                dbQuery = dbQuery.Where(r => r.Status == parsedStatus);
            }

            var result = await dbQuery
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ImportRequestListDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<ImportRequestListDto>>.Success(result);
        }
    }
}
