using Application.ViewModel;
using Domain.Constants;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Application.Features.ImportRequests.Commands.Cancel
{
    public record CancelImportRequestCommand(Guid RequestId) : IRequest<ResponseViewModel<bool>>;

    public class CancelImportRequestCommandHandler
        : IRequestHandler<CancelImportRequestCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CancelImportRequestCommandHandler> _logger;

        public CancelImportRequestCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CancelImportRequestCommandHandler> logger)
        {
            _requestRepo = requestRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            CancelImportRequestCommand command,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var isAdmin = _httpContextAccessor.HttpContext.User.IsInRole(AppRoles.Admin);

            _logger.LogInformation("User {UserId} is requesting cancellation of ImportRequest {RequestId}",
                userId, command.RequestId);

            var importRequest = await _requestRepo.GetByIdAsync(command.RequestId);
            if (importRequest is null)
                throw new NotFoundException("Import request not found.", "ImportRequests");

            // Customer can only cancel their own request
            if (!isAdmin && importRequest.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to cancel this request.");

            // Customers can only cancel Pending requests
            if (!isAdmin && importRequest.Status != RequestStatus.Pending)
                throw new BusinessLogicException(
                    "You can only cancel requests that are still pending.",
                    "ImportRequests");

            // Admin can cancel anything except Shipped (which is locked for everyone including Admin in the domain,
            // but spec says Admin can cancel Shipped — so Admin bypasses this check)
            if (isAdmin && importRequest.Status == RequestStatus.Shipped)
            {
                _logger.LogWarning("Admin {UserId} is force-cancelling a Shipped request {RequestId}",
                    userId, command.RequestId);
            }

            await _requestRepo.UpdatePartialAsync(
                new ImportRequest { Id = importRequest.Id, Status = RequestStatus.Cancelled, UpdatedAt = DateTime.UtcNow },
                nameof(ImportRequest.Status),
                nameof(ImportRequest.UpdatedAt));

            _logger.LogInformation("ImportRequest {RequestId} cancelled by User {UserId}",
                command.RequestId, userId);

            return ResponseViewModel<bool>.Success(true, "Import request cancelled successfully.");
        }
    }
}