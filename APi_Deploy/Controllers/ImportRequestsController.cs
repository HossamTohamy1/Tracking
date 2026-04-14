using Application.DTOs.ImportRequests;
using Application.Features.ImportRequests.Commands.Approve;
using Application.Features.ImportRequests.Commands.AssignOffice;
using Application.Features.ImportRequests.Commands.Cancel;
using Application.Features.ImportRequests.Commands.Reject;
using Application.Features.ImportRequests.Commands.Submit;
using Application.Features.ImportRequests.Commands.UpdateStage;
using Application.Features.ImportRequests.Queries;
using Application.Features.Users.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APi_Presentation.Controllers
{
    [ApiController]
    [Route("api/import-requests")]
    [Authorize]
    public class ImportRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ImportRequestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/import-requests — Customer
        [HttpPost]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Submit([FromBody] SubmitImportRequestDto dto)
        {
            var result = await _mediator.Send(new SubmitImportRequestCommand(dto));
            return Ok(result);
        }
        // GET /api/import-requests — Customer (own requests)
        [HttpGet]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetMy([FromQuery] string? status)
        {
            var result = await _mediator.Send(new GetMyImportRequestsQuery(status));
            return Ok(result);
        }

        // GET /api/import-requests/admin — Admin | Support
        [HttpGet("admin")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Support}")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] Guid? officeId,
            [FromQuery] Guid? userId)
        {
            var result = await _mediator.Send(new GetAllImportRequestsQuery(status, officeId, userId));
            return Ok(result);
        }

        // GET /api/import-requests/office — ImportOffice
        [HttpGet("office")]
        [Authorize(Roles = AppRoles.ImportOffice)]
        public async Task<IActionResult> GetOfficeRequests([FromQuery] string? status)
        {
            var result = await _mediator.Send(new GetOfficeImportRequestsQuery(status));
            return Ok(result);
        }

        // GET /api/import-requests/{id} — Admin | Support | ImportOffice | Customer
        [HttpGet("{id:guid}")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Support},{AppRoles.ImportOffice},{AppRoles.Customer}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetImportRequestByIdQuery(id));
            return Ok(result);
        }

        // DELETE /api/import-requests/{id} — Customer | Admin
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = $"{AppRoles.Customer},{AppRoles.Admin}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _mediator.Send(new CancelImportRequestCommand(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveImportRequestDto dto)
        {
            var result = await _mediator.Send(
                new ApproveImportRequestCommand(id, dto)
            );

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // PATCH /api/import-requests/{id}/reject — ImportOffice
        [HttpPatch("{id:guid}/reject")]
        [Authorize(Roles = AppRoles.ImportOffice)]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectImportRequestDto dto)
        {
            var result = await _mediator.Send(new RejectImportRequestCommand(id, dto));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // PATCH /api/import-requests/{id}/assign-office — Admin
        [HttpPatch("{id:guid}/assign-office")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> AssignOffice(Guid id, [FromBody] AssignOfficeDto dto)
        {
            var result = await _mediator.Send(new AssignOfficeCommand(id, dto));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id:guid}/update-stage")]
        [Authorize(Roles = AppRoles.ImportOffice)]
        public async Task<IActionResult> UpdateStage(Guid id, [FromBody] UpdateStageDto dto)
        {
            var result = await _mediator.Send(new UpdateStageCommand(id, dto));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("offices")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> GetOfficeUsers()
        {
            var result = await _mediator.Send(new GetOfficeUsersQuery());
            return Ok(result);
        }
    }
}
