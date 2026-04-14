using Application.DTOs.ImportRequests;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Constants;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public record GetImportRequestByIdQuery(Guid RequestId) : IRequest<ResponseViewModel<ImportRequestDto>>;

public class GetImportRequestByIdQueryHandler
    : IRequestHandler<GetImportRequestByIdQuery, ResponseViewModel<ImportRequestDto>>
{
    private readonly IGeneralRepository<ImportRequest> _requestRepo;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GetImportRequestByIdQueryHandler> _logger;

    public GetImportRequestByIdQueryHandler(
        IGeneralRepository<ImportRequest> requestRepo,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<GetImportRequestByIdQueryHandler> logger)
    {
        _requestRepo = requestRepo;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ResponseViewModel<ImportRequestDto>> Handle(
        GetImportRequestByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = _httpContextAccessor.HttpContext.User.IsInRole(AppRoles.Admin);
        var isSupport = _httpContextAccessor.HttpContext.User.IsInRole(AppRoles.Support);
        var isOffice = _httpContextAccessor.HttpContext.User.IsInRole(AppRoles.ImportOffice);

        _logger.LogInformation("User {UserId} fetching ImportRequest {RequestId}", userId, query.RequestId);

        var dto = await _requestRepo
            .GetAll()
            .Where(r => r.Id == query.RequestId)
            .ProjectTo<ImportRequestDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            throw new NotFoundException("Import request not found.", "ImportRequests");

        // Customers can only see their own requests
        if (!isAdmin && !isSupport && !isOffice && dto.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this request.");

        // ImportOffice can only see requests assigned to them
        if (isOffice && !isAdmin && !isSupport && dto.AssignedOfficeId != userId)
            throw new UnauthorizedAccessException("This request is not assigned to your office.");

        return ResponseViewModel<ImportRequestDto>.Success(dto);
    }
}
