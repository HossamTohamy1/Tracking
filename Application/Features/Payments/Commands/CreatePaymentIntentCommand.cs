using Application.ViewModel;
using MediatR;

namespace Application.Features.Payments.Commands
{
    public record CreatePaymentIntentCommand(Guid ImportRequestId, string Currency)
        : IRequest<ResponseViewModel<PaymentIntentResponseDto>>;

    public class PaymentIntentResponseDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}