using Application.DTOs.ImportRequests;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums.Enums_Models;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.ImportRequests.Queries
{
    public record GetAllImportRequestsQuery(string? Status, Guid? OfficeId, Guid? UserId)
        : IRequest<ResponseViewModel<List<ImportRequestListDto>>>;

    public class GetAllImportRequestsQueryHandler
        : IRequestHandler<GetAllImportRequestsQuery, ResponseViewModel<List<ImportRequestListDto>>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllImportRequestsQueryHandler> _logger;

        public GetAllImportRequestsQueryHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IMapper mapper,
            ILogger<GetAllImportRequestsQueryHandler> logger)
        {
            _requestRepo = requestRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<ImportRequestListDto>>> Handle(
            GetAllImportRequestsQuery query,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin/Support listing all import requests. Status={Status} OfficeId={OfficeId} UserId={UserId}",
                query.Status, query.OfficeId, query.UserId);

            var dbQuery = _requestRepo.GetAll();

            if (!string.IsNullOrWhiteSpace(query.Status)
                && Enum.TryParse<RequestStatus>(query.Status, ignoreCase: true, out var parsedStatus))
                dbQuery = dbQuery.Where(r => r.Status == parsedStatus);

            if (query.OfficeId.HasValue)
                dbQuery = dbQuery.Where(r => r.AssignedOfficeId == query.OfficeId);

            if (query.UserId.HasValue)
                dbQuery = dbQuery.Where(r => r.UserId == query.UserId);

            var result = await dbQuery
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ImportRequestListDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ResponseViewModel<List<ImportRequestListDto>>.Success(result);
        }
    }
}