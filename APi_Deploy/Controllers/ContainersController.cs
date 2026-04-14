using Application.DTOs.ContainerDtos;
using Application.Features.Containers.Commands;
using Application.Features.Containers.Queries;
using Application.Features.Containers.Queries.Handlers;
using Application.ViewModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APi_Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContainersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ContainersController> _logger;

        public ContainersController(
            IMediator mediator,
            ILogger<ContainersController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // CREATE CONTAINER
        // POST: /api/containers
        // Roles: ImportOffice
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [Authorize(Roles = "ImportOffice")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateContainer(
            [FromBody] CreateContainerRequest request)
        {
            try
            {
               
                var command = new CreateContainerCommand
                {
                    MaxWeightKg = request.MaxWeightKg,
                    MaxVolumeCbm = request.MaxVolumeCbm,
                    OriginPort = request.OriginPort,
                    DestinationPort = request.DestinationPort,
                    ExpectedArrival = request.ExpectedArrival
                };

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(
                        nameof(GetContainerById),
                        new { containerId = result.Data.Id },
                        result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CREATE CONTAINER ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GET CONTAINER BY ID
        // GET: /api/containers/{id}
        // Roles: ImportOffice, Admin, Support, Customer
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet("{containerId}")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetContainerById(Guid containerId)
        {
            try
            {
                _logger.LogInformation(
                    "[GET CONTAINER ENDPOINT] Fetching container. ContainerId: {ContainerId}",
                    containerId);

                var query = new GetContainerByIdQuery { ContainerId = containerId };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET CONTAINER ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GET OFFICE CONTAINERS (ImportOffice Only)
        // GET: /api/containers
        // Roles: ImportOffice
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Roles = "ImportOffice")]
        [ProducesResponseType(typeof(ResponseViewModel<PaginatedResult<ContainerListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOfficeContainers(
            [FromQuery] int? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation(
                    "[GET OFFICE CONTAINERS ENDPOINT] Fetching office containers. Page: {Page}, PageSize: {PageSize}",
                    page,
                    pageSize);

                var query = new GetOfficeContainersQuery
                {
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET OFFICE CONTAINERS ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GET ALL CONTAINERS (Admin/Support)
        // GET: /api/containers/admin
        // Roles: Admin, Support
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,Support")]
        [ProducesResponseType(typeof(ResponseViewModel<PaginatedResult<ContainerListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllContainers(
            [FromQuery] int? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation(
                    "[GET ALL CONTAINERS ENDPOINT] Admin/Support view. Page: {Page}, PageSize: {PageSize}",
                    page,
                    pageSize);

                var query = new GetAllContainersQuery
                {
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET ALL CONTAINERS ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // CLOSE CONTAINER
        // PATCH: /api/containers/{id}/close
        // Roles: ImportOffice
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPatch("{containerId}/close")]
        [Authorize(Roles = "ImportOffice")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CloseContainer(Guid containerId)
        {
            try
            {
                _logger.LogInformation(
                    "[CLOSE CONTAINER ENDPOINT] Closing container. ContainerId: {ContainerId}",
                    containerId);

                var command = new CloseContainerCommand { ContainerId = containerId };
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CLOSE CONTAINER ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UPDATE CONTAINER
        // PUT: /api/containers/{id}
        // Roles: ImportOffice
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPut("{containerId}")]
        [Authorize(Roles = "ImportOffice")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContainer(
            Guid containerId,
            [FromBody] UpdateContainerRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE CONTAINER ENDPOINT] Updating container. ContainerId: {ContainerId}",
                    containerId);

                var command = new UpdateContainerCommand
                {
                    Id = containerId,
                    ContainerNumber = request.ContainerNumber,
                    MaxWeightKg = request.MaxWeightKg,
                    MaxVolumeCbm = request.MaxVolumeCbm,
                    OriginPort = request.OriginPort,
                    DestinationPort = request.DestinationPort,
                    ExpectedArrival = request.ExpectedArrival
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UPDATE CONTAINER ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UPDATE CONTAINER STATUS
        // PATCH: /api/containers/{id}/status
        // Roles: ImportOffice, Admin, Support
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPatch("{containerId}/status")]
        [Authorize(Roles = "ImportOffice,Admin,Support")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContainerStatus(
            Guid containerId,
            [FromBody] UpdateContainerStatusRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE CONTAINER STATUS ENDPOINT] Updating status. ContainerId: {ContainerId}, Status: {Status}",
                    containerId,
                    request.Status);

                var command = new UpdateContainerStatusCommand
                {
                    ContainerId = containerId,
                    Status = request.Status
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UPDATE CONTAINER STATUS ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UPDATE SHIPPING COST
        // PATCH: /api/containers/{id}/shipping-cost
        // Roles: ImportOffice, Admin
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPatch("{containerId}/shipping-cost")]
        [Authorize(Roles = "ImportOffice,Admin")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateShippingCost(
            Guid containerId,
            [FromBody] UpdateContainerShippingCostRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE SHIPPING COST ENDPOINT] Updating shipping cost. ContainerId: {ContainerId}, Cost: {Cost}",
                    containerId,
                    request.TotalShippingCost);

                var command = new UpdateContainerShippingCostCommand
                {
                    ContainerId = containerId,
                    TotalShippingCost = request.TotalShippingCost
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UPDATE SHIPPING COST ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // REMOVE CONTAINER ITEM
        // DELETE: /api/containers/{id}/items/{itemId}
        // Roles: ImportOffice, Admin
        // ═══════════════════════════════════════════════════════════════════════
        [HttpDelete("{containerId}/items/{itemId}")]
        [Authorize(Roles = "ImportOffice,Admin")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveContainerItem(
            Guid containerId,
            Guid itemId)
        {
            try
            {
                _logger.LogInformation(
                    "[REMOVE CONTAINER ITEM ENDPOINT] Removing item. ContainerId: {ContainerId}, ItemId: {ItemId}",
                    containerId,
                    itemId);

                var command = new RemoveContainerItemCommand
                {
                    ContainerId = containerId,
                    ItemId = itemId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REMOVE CONTAINER ITEM ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GET COST BREAKDOWN
        // GET: /api/containers/{id}/cost-breakdown
        // Roles: Admin, ImportOffice, Support
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet("{containerId}/cost-breakdown")]
        [Authorize(Roles = "Admin,ImportOffice,Support")]
        [ProducesResponseType(typeof(ResponseViewModel<ContainerCostBreakdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCostBreakdown(Guid containerId)
        {
            try
            {
                _logger.LogInformation(
                    "[GET COST BREAKDOWN ENDPOINT] Fetching cost breakdown. ContainerId: {ContainerId}",
                    containerId);

                var query = new GetContainerCostBreakdownQuery { ContainerId = containerId };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET COST BREAKDOWN ENDPOINT] Unexpected error");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // DELETE CONTAINER (SOFT DELETE)
        // DELETE: /api/containers/{id}
        // Roles: Admin
        // ═══════════════════════════════════════════════════════════════════════
        [HttpDelete("{containerId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ResponseViewModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseViewModel<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteContainer(Guid containerId)
        {
            try
            {
                _logger.LogInformation(
                    "[DELETE CONTAINER ENDPOINT] Deleting container. ContainerId: {ContainerId}",
                    containerId);

                var command = new DeleteContainerCommand { ContainerId = containerId };
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DELETE CONTAINER ENDPOINT] Unexpected error");
                throw;
            }
        }
        // GET /api/containers/suggestions?requestId=...
        [HttpGet("suggestions")]
        [Authorize(Roles = "ImportOffice,Admin")]
        public async Task<IActionResult> GetSuggestions([FromQuery] Guid requestId)
        {
            var result = await _mediator.Send(new GetContainerSuggestionsQuery
            {
                ImportRequestId = requestId
            });
            return Ok(result);
        }

        // POST /api/containers/assign
        [HttpPost("assign")]
        [Authorize(Roles = "ImportOffice,Admin")]
        public async Task<IActionResult> Assign([FromBody] AssignContainerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
