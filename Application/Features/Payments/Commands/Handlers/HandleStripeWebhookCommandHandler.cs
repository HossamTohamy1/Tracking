using Application.ViewModel;
using Domain.Enums.Enums_Models;
using Domain.Events; 
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Features.Payments.Commands.Handlers
{
    public class HandleStripeWebhookCommandHandler
        : IRequestHandler<HandleStripeWebhookCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Payment> _paymentRepo;
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HandleStripeWebhookCommandHandler> _logger;

        public HandleStripeWebhookCommandHandler(
            IGeneralRepository<Payment> paymentRepo,
            IGeneralRepository<ImportRequest> requestRepo,
            IConfiguration configuration,
            ILogger<HandleStripeWebhookCommandHandler> logger)
        {
            _paymentRepo = paymentRepo;
            _requestRepo = requestRepo;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            HandleStripeWebhookCommand command,
            CancellationToken cancellationToken)
        {
            // ── 1. Verify Stripe Signature ─────────────────────────────────
            var webhookSecret = _configuration["Stripe:WebhookSecret"]
                ?? throw new BusinessLogicException("Stripe webhook secret not configured.", "Webhook");

            if (!VerifyStripeSignature(command.RawBody, command.StripeSignature, webhookSecret))
            {
                _logger.LogWarning("[STRIPE WEBHOOK] Invalid signature. Possible tampered request.");
                throw new BusinessLogicException("Invalid Stripe signature.", "Webhook");
            }

            // ── 2. Deserialize Payload using YOUR Event class ──────────────
            StripeWebhookPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<StripeWebhookPayload>(
                    command.RawBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[STRIPE WEBHOOK] Failed to deserialize payload.");
                throw new BusinessLogicException("Invalid webhook payload.", "Webhook");
            }

            if (payload is null)
                throw new BusinessLogicException("Empty webhook payload.", "Webhook");

            _logger.LogInformation("[STRIPE WEBHOOK] Received event: {Type}", payload.Type);

            // ── 3. Route by Event Type ─────────────────────────────────────
            switch (payload.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentSucceededAsync(payload, cancellationToken);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentFailedAsync(payload, cancellationToken);
                    break;

                default:
                    _logger.LogInformation("[STRIPE WEBHOOK] Unhandled event type: {Type}", payload.Type);
                    break;
            }

            return ResponseViewModel<bool>.Success(true, "Webhook processed successfully.");
        }

        private async Task HandlePaymentSucceededAsync(
            StripeWebhookPayload payload,
            CancellationToken cancellationToken)
        {
            // ✅ استخدم الـ object بتاعك: payload.Data.Object
            var paymentIntent = payload.Data.Object;

            _logger.LogInformation(
                "[STRIPE WEBHOOK] payment_intent.succeeded. PaymentIntentId: {Id}, Amount: {Amount}, Currency: {Currency}",
                paymentIntent.Id, paymentIntent.Amount, paymentIntent.Currency);

            var payment = _paymentRepo.GetAll()
                .FirstOrDefault(p => p.GatewayTransactionId == paymentIntent.Id);

            if (payment is null)
            {
                _logger.LogWarning(
                    "[STRIPE WEBHOOK] Payment not found for PaymentIntentId: {Id}. Maybe created externally.",
                    paymentIntent.Id);
                return;
            }

            await _paymentRepo.UpdatePartialAsync(
                new Payment
                {
                    Id = payment.Id,
                    Status = PaymentStatus.Completed,
                    PaidAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                nameof(Payment.Status),
                nameof(Payment.PaidAt),
                nameof(Payment.UpdatedAt));

            _logger.LogInformation(
                "[STRIPE WEBHOOK] Payment {PaymentId} marked as Completed.", payment.Id);

            if (payment.ImportRequestId.HasValue)
            {
                var importRequest = await _requestRepo.GetByIdAsync(payment.ImportRequestId.Value);
                if (importRequest is not null && importRequest.Status == RequestStatus.Approved)
                {
                    await _requestRepo.UpdatePartialAsync(
                        new ImportRequest
                        {
                            Id = importRequest.Id,
                            Status = RequestStatus.Processing,
                            UpdatedAt = DateTime.UtcNow
                        },
                        nameof(ImportRequest.Status),
                        nameof(ImportRequest.UpdatedAt));

                    _logger.LogInformation(
                        "[STRIPE WEBHOOK] ImportRequest {ReqId} moved to Processing after payment.",
                        importRequest.Id);
                }
            }
        }

        private async Task HandlePaymentFailedAsync(
            StripeWebhookPayload payload,
            CancellationToken cancellationToken)
        {
            var paymentIntent = payload.Data.Object;

            _logger.LogWarning(
                "[STRIPE WEBHOOK] payment_intent.payment_failed. PaymentIntentId: {Id}",
                paymentIntent.Id);

            var payment = _paymentRepo.GetAll()
                .FirstOrDefault(p => p.GatewayTransactionId == paymentIntent.Id);

            if (payment is null)
            {
                _logger.LogWarning(
                    "[STRIPE WEBHOOK] Payment not found for failed PaymentIntentId: {Id}",
                    paymentIntent.Id);
                return;
            }

            await _paymentRepo.UpdatePartialAsync(
                new Payment
                {
                    Id = payment.Id,
                    Status = PaymentStatus.Failed,
                    FailureReason = $"Stripe reported payment failure. IntentId: {paymentIntent.Id}",
                    UpdatedAt = DateTime.UtcNow
                },
                nameof(Payment.Status),
                nameof(Payment.FailureReason),
                nameof(Payment.UpdatedAt));

            _logger.LogWarning(
                "[STRIPE WEBHOOK] Payment {PaymentId} marked as Failed.", payment.Id);
        }

        private static bool VerifyStripeSignature(string rawBody, string signatureHeader, string secret)
        {
            try
            {
                var parts = signatureHeader.Split(',');
                var timestamp = parts.FirstOrDefault(p => p.StartsWith("t="))?.Substring(2);
                var signature = parts.FirstOrDefault(p => p.StartsWith("v1="))?.Substring(3);

                if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
                    return false;

                var signedPayload = $"{timestamp}.{rawBody}";
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

                using var hmac = new HMACSHA256(secretBytes);
                var computedHash = hmac.ComputeHash(payloadBytes);
                var computedSignature = BitConverter.ToString(computedHash)
                    .Replace("-", "")
                    .ToLower();

                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(computedSignature),
                    Encoding.UTF8.GetBytes(signature));
            }
            catch
            {
                return false;
            }
        }
    }
}