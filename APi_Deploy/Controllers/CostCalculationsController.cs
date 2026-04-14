using Application.DTOs.CostCalculationDtos;
using Application.Features.CostCalculations.Commands;
using Application.Features.CostCalculations.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APi_Presentation.Controllers
{
    [ApiController]
    [Route("api/cost-calculations")]
    [Authorize]
    public class CostCalculationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CostCalculationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ── GET /api/cost-calculations/admin  (Admin | Support | ImportOffice) ──
        [HttpGet("admin")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Support},{AppRoles.ImportOffice}")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? currency = null)
        {
            var result = await _mediator.Send(
                new GetAllCostCalculationsQuery(page, pageSize, currency));
            return Ok(result);
        }

        // ── GET /api/cost-calculations/my  (Customer) ────────────────────────
        [HttpGet("my")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetMyCostCalculations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? currency = null)
        {
            var result = await _mediator.Send(
                new GetMyCostCalculationsQuery(page, pageSize, currency));
            return Ok(result);
        }

        // ── GET /api/cost-calculations/{requestId}  (Admin|Support|Office|Customer) ──
        [HttpGet("{requestId:guid}")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Support},{AppRoles.ImportOffice},{AppRoles.Customer}")]
        public async Task<IActionResult> GetByRequestId(Guid requestId)
        {
            var result = await _mediator.Send(
                new GetCostCalculationByRequestIdQuery(requestId));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // ── PUT /api/cost-calculations/{requestId}  (ImportOffice) ───────────
        [HttpPut("{requestId:guid}")]
        [Authorize(Roles = AppRoles.ImportOffice)]
        public async Task<IActionResult> UpdateCostComponents(
            Guid requestId,
            [FromBody] UpdateCostCalculationRequest dto)
        {
            var result = await _mediator.Send(
                new UpdateCostCalculationCommand(requestId, dto));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // ── PATCH /api/cost-calculations/{requestId}/discount  (Admin|ImportOffice) ──
        [HttpPatch("{requestId:guid}/discount")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ImportOffice}")]
        public async Task<IActionResult> UpdateDiscount(
            Guid requestId,
            [FromBody] UpdateDiscountRequest dto)
        {
            var result = await _mediator.Send(
                new UpdateCostCalculationDiscountCommand(requestId, dto));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}