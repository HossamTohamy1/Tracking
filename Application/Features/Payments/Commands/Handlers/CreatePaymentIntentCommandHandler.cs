using Application.ViewModel;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Security.Claims;

namespace Application.Features.Payments.Commands.Handlers
{
    public class CreatePaymentIntentCommandHandler
        : IRequestHandler<CreatePaymentIntentCommand, ResponseViewModel<PaymentIntentResponseDto>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IGeneralRepository<Domain.Models.Payment> _paymentRepo;
        private readonly IGeneralRepository<CostCalculation> _costRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreatePaymentIntentCommandHandler> _logger;

        // ── Supported currencies by Stripe ───────────────────────────────────
        private static readonly HashSet<string> _supportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
        {
            "USD", "EUR", "GBP", "EGP", "SAR", "AED", "KWD", "QAR", "BHD", "OMR", "JOD"
        };

        public CreatePaymentIntentCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IGeneralRepository<Domain.Models.Payment> paymentRepo,
            IGeneralRepository<CostCalculation> costRepo,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<CreatePaymentIntentCommandHandler> logger)
        {
            _requestRepo = requestRepo ?? throw new ArgumentNullException(nameof(requestRepo));
            _paymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
            _costRepo = costRepo ?? throw new ArgumentNullException(nameof(costRepo));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<PaymentIntentResponseDto>> Handle(
            CreatePaymentIntentCommand command,
            CancellationToken cancellationToken)
        {
            // ══════════════════════════════════════════════════════════════════
            // STEP 0: Validate Command Input
            // ══════════════════════════════════════════════════════════════════
            if (command is null)
            {
                _logger.LogError("[CREATE PAYMENT INTENT] Command is null.");
                throw new BusinessLogicException("Invalid payment request.", "Payments");
            }

            if (command.ImportRequestId == Guid.Empty)
            {
                _logger.LogWarning("[CREATE PAYMENT INTENT] ImportRequestId is empty GUID.");
                throw new BusinessLogicException("Import request ID is required.", "Payments");
            }

            var currency = (command.Currency ?? "USD").Trim().ToUpper();

            if (!_supportedCurrencies.Contains(currency))
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] Unsupported currency: {Currency}", currency);
                throw new BusinessLogicException(
                    $"Currency '{currency}' is not supported. Supported currencies: {string.Join(", ", _supportedCurrencies)}.",
                    "Payments");
            }

            // ══════════════════════════════════════════════════════════════════
            // STEP 1: Extract UserId from JWT
            // ══════════════════════════════════════════════════════════════════
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                _logger.LogError("[CREATE PAYMENT INTENT] Could not extract userId from JWT.");
                throw new BusinessLogicException(
                    "Authentication failed. Could not identify current user.", "Payments");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogError(
                    "[CREATE PAYMENT INTENT] userId claim is not a valid GUID: {Claim}", userIdClaim);
                throw new BusinessLogicException(
                    "Authentication token is malformed.", "Payments");
            }

            _logger.LogInformation(
                "[CREATE PAYMENT INTENT] Starting. UserId: {UserId}, ImportRequestId: {RequestId}, Currency: {Currency}",
                userId, command.ImportRequestId, currency);

            // ══════════════════════════════════════════════════════════════════
            // STEP 2: Load & Validate ImportRequest
            // ══════════════════════════════════════════════════════════════════
            ImportRequest importRequest;
            try
            {
                importRequest = await _requestRepo.GetByIdAsync(command.ImportRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CREATE PAYMENT INTENT] DB error while loading ImportRequest {RequestId}.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "Failed to load import request. Please try again.", "Payments");
            }

            if (importRequest is null)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] ImportRequest {RequestId} not found.",
                    command.ImportRequestId);
                throw new NotFoundException(
                    $"Import request '{command.ImportRequestId}' not found.", "Payments");
            }

            // ── Ownership check ───────────────────────────────────────────────
            if (importRequest.UserId != userId)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] User {UserId} tried to pay for request owned by {OwnerId}. Possible IDOR.",
                    userId, importRequest.UserId);
                throw new BusinessLogicException(
                    "You are not authorized to pay for this import request.", "Payments");
            }

            // ── Status check ──────────────────────────────────────────────────
            if (importRequest.Status == RequestStatus.Cancelled)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] ImportRequest {RequestId} is cancelled. Cannot pay.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "This import request has been cancelled and cannot be paid.", "Payments");
            }

            if (importRequest.Status == RequestStatus.Rejected)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] ImportRequest {RequestId} is rejected. Cannot pay.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "This import request was rejected. Payment is not allowed.", "Payments");
            }

            if (importRequest.Status == RequestStatus.Pending)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] ImportRequest {RequestId} is still Pending. Waiting for office approval.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "Your import request is still pending office approval. Payment is not available yet.", "Payments");
            }

            if (importRequest.Status == RequestStatus.Processing ||
                importRequest.Status == RequestStatus.Shipped ||
                importRequest.Status == RequestStatus.Delivered)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] ImportRequest {RequestId} status is {Status}. Payment already completed.",
                    command.ImportRequestId, importRequest.Status);
                throw new BusinessLogicException(
                    $"This request is already in '{importRequest.Status}' status. Payment has already been processed.", "Payments");
            }

            if (importRequest.Status != RequestStatus.Approved)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] ImportRequest {RequestId} has unexpected status: {Status}.",
                    command.ImportRequestId, importRequest.Status);
                throw new BusinessLogicException(
                    $"Payment is only allowed for approved requests. Current status: '{importRequest.Status}'.", "Payments");
            }

            // ══════════════════════════════════════════════════════════════════
            // STEP 3: Check if Payment Already Exists (Idempotency Guard)
            // ══════════════════════════════════════════════════════════════════
            IQueryable<Domain.Models.Payment> existingPaymentsQuery;
            try
            {
                existingPaymentsQuery = _paymentRepo.GetAll()
                    .Where(p => p.ImportRequestId == command.ImportRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CREATE PAYMENT INTENT] DB error while checking existing payments for RequestId {RequestId}.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "Failed to verify payment status. Please try again.", "Payments");
            }

            // ── Check for already completed payment ───────────────────────────
            var completedPayment = existingPaymentsQuery
                .FirstOrDefault(p => p.Status == PaymentStatus.Completed);

            if (completedPayment is not null)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] Payment {PaymentId} already completed for RequestId {RequestId}.",
                    completedPayment.Id, command.ImportRequestId);
                throw new BusinessLogicException(
                    "A payment has already been completed for this import request. No further payment is needed.", "Payments");
            }

            // ── Check for existing pending payment (return existing ClientSecret instead of creating new) ──
            var pendingPayment = existingPaymentsQuery
                .FirstOrDefault(p => p.Status == PaymentStatus.Pending
                                     && p.GatewayTransactionId != null);

            if (pendingPayment is not null)
            {
                _logger.LogInformation(
                    "[CREATE PAYMENT INTENT] Found existing Pending payment {PaymentId} for RequestId {RequestId}. " +
                    "Fetching existing PaymentIntent from Stripe.",
                    pendingPayment.Id, command.ImportRequestId);

                // Try to fetch the existing intent from Stripe instead of creating a new one
                try
                {
                    StripeConfiguration.ApiKey = GetStripeSecretKey();
                    var existingService = new PaymentIntentService();
                    var existingIntent = await existingService.GetAsync(
                        pendingPayment.GatewayTransactionId,
                        cancellationToken: cancellationToken);

                    if (existingIntent?.Status is "requires_payment_method" or "requires_confirmation" or "requires_action")
                    {
                        _logger.LogInformation(
                            "[CREATE PAYMENT INTENT] Returning existing PaymentIntent {IntentId} (status: {Status}).",
                            existingIntent.Id, existingIntent.Status);

                        return ResponseViewModel<PaymentIntentResponseDto>.Success(
                            new PaymentIntentResponseDto
                            {
                                PaymentId = pendingPayment.Id.ToString(),
                                ClientSecret = existingIntent.ClientSecret,
                                Amount = pendingPayment.Amount,
                                Currency = pendingPayment.Currency,
                                Status = existingIntent.Status
                            },
                            "Existing payment intent returned. Please complete the payment.");
                    }
                }
                catch (StripeException stripeEx)
                {
                    // إذا الـ intent مش موجود على Stripe — نكمل وننشئ واحد جديد
                    _logger.LogWarning(stripeEx,
                        "[CREATE PAYMENT INTENT] Could not retrieve existing Stripe intent {IntentId}. Will create new one.",
                        pendingPayment.GatewayTransactionId);
                }
            }

            // ══════════════════════════════════════════════════════════════════
            // STEP 4: Load & Validate CostCalculation
            // ══════════════════════════════════════════════════════════════════
            CostCalculation cost;
            try
            {
                cost = _costRepo.GetAll()
                    .FirstOrDefault(c => c.ImportRequestId == command.ImportRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CREATE PAYMENT INTENT] DB error while loading CostCalculation for RequestId {RequestId}.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "Failed to load cost calculation. Please try again.", "Payments");
            }

            if (cost is null)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] No CostCalculation found for RequestId {RequestId}.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "Cost calculation has not been created yet. Please wait for the import office to process your request.",
                    "Payments");
            }

            if (cost.FinalAmount <= 0)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] CostCalculation FinalAmount is {Amount} for RequestId {RequestId}. Not ready.",
                    cost.FinalAmount, command.ImportRequestId);
                throw new BusinessLogicException(
                    "Cost calculation is not finalized yet. The import office has not entered the final amount. Please try again later.",
                    "Payments");
            }

            // ── Guard against extremely high amounts (anti-fraud) ─────────────
            if (cost.FinalAmount > 1_000_000)
            {
                _logger.LogError(
                    "[CREATE PAYMENT INTENT] Suspiciously high FinalAmount {Amount} for RequestId {RequestId}. Blocking.",
                    cost.FinalAmount, command.ImportRequestId);
                throw new BusinessLogicException(
                    "Payment amount exceeds the maximum allowed limit. Please contact support.", "Payments");
            }

            // ══════════════════════════════════════════════════════════════════
            // STEP 5: Validate Stripe Configuration
            // ══════════════════════════════════════════════════════════════════
            string stripeSecretKey;
            try
            {
                stripeSecretKey = GetStripeSecretKey();
            }
            catch (BusinessLogicException)
            {
                throw;
            }

            // ══════════════════════════════════════════════════════════════════
            // STEP 6: Create Stripe PaymentIntent
            // ══════════════════════════════════════════════════════════════════
            StripeConfiguration.ApiKey = stripeSecretKey;

            var amountInCents = (long)Math.Round(cost.FinalAmount * 100);

            _logger.LogInformation(
                "[CREATE PAYMENT INTENT] Creating Stripe PaymentIntent. Amount: {Amount} cents, Currency: {Currency}",
                amountInCents, currency.ToLower());

            PaymentIntent intent;
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currency.ToLower(),
                    // automatic_payment_methods يدعم كل طرق الدفع المتاحة بدون ما تحددهم manually
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        ["importRequestId"] = command.ImportRequestId.ToString(),
                        ["userId"] = userId.ToString(),
                        ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                    },
                    // بيحط description في Stripe Dashboard عشان يسهل البحث
                    Description = $"Import Shipment Payment - Request {command.ImportRequestId}"
                };

                var service = new PaymentIntentService();
                intent = await service.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx,
                    "[CREATE PAYMENT INTENT] Stripe API error. Code: {Code}, Message: {Message}",
                    stripeEx.StripeError?.Code, stripeEx.Message);

                // ── ترجمة أخطاء Stripe لرسائل مفهومة للمستخدم ──
                var userFriendlyMessage = stripeEx.StripeError?.Code switch
                {
                    "card_declined" => "Your card was declined. Please try a different payment method.",
                    "insufficient_funds" => "Insufficient funds. Please use a different card.",
                    "invalid_api_key" => "Payment service configuration error. Please contact support.",
                    "amount_too_small" => $"The payment amount is too small for the selected currency ({currency}).",
                    "amount_too_large" => "The payment amount exceeds the maximum allowed. Please contact support.",
                    "currency_not_supported" => $"Currency '{currency}' is not supported for your account.",
                    "api_connection_error" => "Could not connect to payment service. Please check your internet and try again.",
                    "rate_limit" => "Payment service is busy. Please wait a moment and try again.",
                    _ => "Payment processing failed. Please try again or contact support."
                };

                throw new BusinessLogicException(userFriendlyMessage, "Payments");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning(
                    "[CREATE PAYMENT INTENT] Request was cancelled while creating Stripe intent for RequestId {RequestId}.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "Payment request was cancelled. Please try again.", "Payments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CREATE PAYMENT INTENT] Unexpected error while creating Stripe PaymentIntent for RequestId {RequestId}.",
                    command.ImportRequestId);
                throw new BusinessLogicException(
                    "An unexpected error occurred while creating the payment. Please try again.", "Payments");
            }

            // ── Validate Stripe response ──────────────────────────────────────
            if (string.IsNullOrWhiteSpace(intent?.ClientSecret))
            {
                _logger.LogError(
                    "[CREATE PAYMENT INTENT] Stripe returned PaymentIntent without ClientSecret. IntentId: {IntentId}",
                    intent?.Id);
                throw new BusinessLogicException(
                    "Payment service returned an invalid response. Please try again.", "Payments");
            }

            _logger.LogInformation(
                "[CREATE PAYMENT INTENT] Stripe PaymentIntent created. IntentId: {IntentId}, Status: {Status}",
                intent.Id, intent.Status);

            // ══════════════════════════════════════════════════════════════════
            // STEP 7: Save Payment Record to DB
            // ══════════════════════════════════════════════════════════════════
            var payment = new Domain.Models.Payment
            {
                UserId = userId,
                ImportRequestId = command.ImportRequestId,
                Purpose = PaymentPurpose.ImportShipment,
                Status = PaymentStatus.Pending,
                Method = Domain.Enums.Enums_Models.PaymentMethod.Stripe,
                Amount = cost.FinalAmount,
                Currency = currency,
                GatewayTransactionId = intent.Id,
                GatewayResponse = null, // يتملى لما Webhook يرجع
                Notes = $"PaymentIntent created at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
            };

            try
            {
                await _paymentRepo.AddAsync(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CREATE PAYMENT INTENT] DB error while saving Payment record. IntentId: {IntentId}",
                    intent.Id);

                _logger.LogCritical(
                    "[CREATE PAYMENT INTENT] ORPHANED STRIPE INTENT — IntentId: {IntentId}, " +
                    "ImportRequestId: {RequestId}, UserId: {UserId}. Manual reconciliation required.",
                    intent.Id, command.ImportRequestId, userId);

                throw new BusinessLogicException(
                    "Payment was initiated with Stripe but could not be saved. Please contact support and provide your import request ID.",
                    "Payments");
            }

            _logger.LogInformation(
                "[CREATE PAYMENT INTENT] Payment record saved. PaymentId: {PaymentId}, IntentId: {IntentId}",
                payment.Id, intent.Id);

            // ══════════════════════════════════════════════════════════════════
            // STEP 8: Return Success Response
            // ══════════════════════════════════════════════════════════════════
            return ResponseViewModel<PaymentIntentResponseDto>.Success(
                new PaymentIntentResponseDto
                {
                    PaymentId = payment.Id.ToString(),
                    ClientSecret = intent.ClientSecret,
                    Amount = cost.FinalAmount,
                    Currency = currency,
                    Status = intent.Status
                },
                "Payment intent created successfully. Please complete the payment using the provided client secret.");
        }

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// يجيب الـ Stripe SecretKey من الـ configuration مع validation كامل
        /// </summary>
        private string GetStripeSecretKey()
        {
            var secretKey = _configuration["Stripe:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogError("[CREATE PAYMENT INTENT] Stripe:SecretKey is not configured.");
                throw new BusinessLogicException(
                    "Payment service is not configured. Please contact support.", "Payments");
            }

            // ── Basic format validation ───────────────────────────────────────
            if (!secretKey.StartsWith("sk_test_") && !secretKey.StartsWith("sk_live_"))
            {
                _logger.LogError(
                    "[CREATE PAYMENT INTENT] Stripe:SecretKey has invalid format (not sk_test_ or sk_live_).");
                throw new BusinessLogicException(
                    "Payment service configuration is invalid. Please contact support.", "Payments");
            }

            return secretKey;
        }
    }
}