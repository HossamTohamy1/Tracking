using Application.Features.Containers.Commands;
using Application.ViewModel;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands.Handlers
{
    public class DeleteContainerCommandHandler : IRequestHandler<DeleteContainerCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Domain.Models.Container> _containerRepo;
        private readonly ILogger<DeleteContainerCommandHandler> _logger;

        public DeleteContainerCommandHandler(
            IGeneralRepository<Domain.Models.Container> containerRepo,
            ILogger<DeleteContainerCommandHandler> logger)
        {
            _containerRepo = containerRepo ?? throw new ArgumentNullException(nameof(containerRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteContainerCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[DELETE CONTAINER] Deleting container. ContainerId: {ContainerId}",
                    request.ContainerId);

                var container = await _containerRepo.GetByIdAsync(request.ContainerId);
                if (container == null)
                {
                    _logger.LogWarning(
                        "[DELETE CONTAINER] Container not found. ContainerId: {ContainerId}",
                        request.ContainerId);

                    throw new NotFoundException(
                        $"Container with ID {request.ContainerId} not found",
                        "Container");
                }

                await _containerRepo.DeleteAsync(request.ContainerId);

                _logger.LogInformation(
                    "[DELETE CONTAINER] Container deleted (soft-delete). ContainerId: {ContainerId}",
                    request.ContainerId);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Container deleted successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DELETE CONTAINER] Unexpected error. ContainerId: {ContainerId}",
                    request.ContainerId);

                throw new BusinessLogicException(
                    "Failed to delete container",
                    ex,
                    "Container");
            }
        }
    }
}