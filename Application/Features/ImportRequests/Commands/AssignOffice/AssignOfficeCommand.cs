using Application.DTOs.ImportRequests;
using Application.ViewModel;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.ImportRequests.Commands.AssignOffice
{
    public record AssignOfficeCommand(Guid RequestId, AssignOfficeDto Dto) : IRequest<ResponseViewModel<bool>>;

    public class AssignOfficeCommandHandler
        : IRequestHandler<AssignOfficeCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly UserManager<ApplicationUser> _userManager; // ← changed
        private readonly ILogger<AssignOfficeCommandHandler> _logger;

        public AssignOfficeCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            UserManager<ApplicationUser> userManager,            // ← changed
            ILogger<AssignOfficeCommandHandler> logger)
        {
            _requestRepo = requestRepo;
            _userManager = userManager;                          // ← changed
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            AssignOfficeCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin is assigning Office {OfficeId} to ImportRequest {RequestId}",
                command.Dto.OfficeId, command.RequestId);

            var importRequest = await _requestRepo.GetByIdAsync(command.RequestId);
            if (importRequest is null)
                throw new NotFoundException("Import request not found.", "ImportRequests");

            // Use UserManager instead of generic repo
            var office = await _userManager.FindByIdAsync(command.Dto.OfficeId.ToString());
            if (office is null || !office.IsActive)
                throw new NotFoundException("Import office not found or inactive.", "ImportRequests");

            await _requestRepo.UpdatePartialAsync(
                new ImportRequest
                {
                    Id = importRequest.Id,
                    AssignedOfficeId = command.Dto.OfficeId,
                    UpdatedAt = DateTime.UtcNow
                },
                nameof(ImportRequest.AssignedOfficeId),
                nameof(ImportRequest.UpdatedAt));

            _logger.LogInformation("Office {OfficeId} assigned to ImportRequest {RequestId}",
                command.Dto.OfficeId, command.RequestId);

            return ResponseViewModel<bool>.Success(true, "Office assigned successfully.");
        }
    }
}