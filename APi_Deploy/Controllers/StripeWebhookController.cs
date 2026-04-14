using Application.DTOs.PaymentDtos;
using Application.Features.Payments.Commands;
using Application.Features.Payments.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APi_Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/payments/create-intent  (Customer)
        [HttpPost("create-intent")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> CreateIntent(
            [FromBody] CreatePaymentIntentRequest dto)
        {
            var result = await _mediator.Send(
                new CreatePaymentIntentCommand(dto.ImportRequestId, dto.Currency ?? "USD"));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // GET /api/payments/{paymentId}
        [HttpGet("{paymentId:guid}")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Support},{AppRoles.Customer}")]
        public async Task<IActionResult> GetById(Guid paymentId)
        {
            var result = await _mediator.Send(new GetPaymentByIdQuery(paymentId));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        // GET /api/payments/request/{requestId}
        [HttpGet("request/{requestId:guid}")]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Support},{AppRoles.Customer}")]
        public async Task<IActionResult> GetByRequestId(Guid requestId)
        {
            var result = await _mediator.Send(new GetPaymentByRequestIdQuery(requestId));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

      
        // ────────────────────────────────────────────────────────────────────
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            Request.EnableBuffering();

            string rawBody;
            using (var reader = new StreamReader(
                       Request.Body,
                       encoding: System.Text.Encoding.UTF8,
                       detectEncodingFromByteOrderMarks: false,
                       leaveOpen: true))
            {
                rawBody = await reader.ReadToEndAsync();
            }

            Request.Body.Position = 0;

            var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrWhiteSpace(stripeSignature))
                return BadRequest(new { message = "Missing Stripe-Signature header." });

            if (string.IsNullOrWhiteSpace(rawBody))
                return BadRequest(new { message = "Empty request body." });

            var result = await _mediator.Send(new HandleStripeWebhookCommand
            {
                RawBody = rawBody,
                StripeSignature = stripeSignature
            });

            return result.IsSuccess ? Ok() : BadRequest();
        }
    }
}