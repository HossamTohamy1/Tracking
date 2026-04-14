using Application.DTOs.ImportRequests;
using Application.ViewModel;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Application.Features.ImportRequests.Commands.Reject
{
    public record RejectImportRequestCommand(Guid RequestId, RejectImportRequestDto Dto)
        : IRequest<ResponseViewModel<bool>>;

    public class RejectImportRequestCommandHandler
        : IRequestHandler<RejectImportRequestCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RejectImportRequestCommandHandler> _logger;

        public RejectImportRequestCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RejectImportRequestCommandHandler> logger)
        {
            _requestRepo = requestRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            RejectImportRequestCommand command,
            CancellationToken cancellationToken)
        {
            var officeId = Guid.Parse(
                _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _logger.LogInformation("Office {OfficeId} is rejecting ImportRequest {RequestId}",
                officeId, command.RequestId);

            if (string.IsNullOrWhiteSpace(command.Dto.RejectionReason))
                throw new Domain.Exceptions.ValidationException(
                    "Rejection reason is required.",
                    new Dictionary<string, string[]>
                    {
                        ["RejectionReason"] = ["A rejection reason must be provided."]
                    },
                    "ImportRequests");

            var importRequest = await _requestRepo.GetByIdAsync(command.RequestId);
            if (importRequest is null)
                throw new NotFoundException("Import request not found.", "ImportRequests");

            if (importRequest.AssignedOfficeId != officeId)
                throw new UnauthorizedAccessException("You are not the assigned office for this request.");

            if (importRequest.Status != RequestStatus.Pending)
                throw new BusinessLogicException(
                    "Only pending requests can be rejected.",
                    "ImportRequests");

            await _requestRepo.UpdatePartialAsync(
                new ImportRequest
                {
                    Id = importRequest.Id,
                    Status = RequestStatus.Rejected,
                    RejectionReason = command.Dto.RejectionReason,
                    UpdatedAt = DateTime.UtcNow
                },
                nameof(ImportRequest.Status),
                nameof(ImportRequest.RejectionReason),
                nameof(ImportRequest.UpdatedAt));

            _logger.LogInformation("ImportRequest {RequestId} rejected by Office {OfficeId}. Reason: {Reason}",
                command.RequestId, officeId, command.Dto.RejectionReason);

            return ResponseViewModel<bool>.Success(true, "Import request rejected.");
        }
    }
}