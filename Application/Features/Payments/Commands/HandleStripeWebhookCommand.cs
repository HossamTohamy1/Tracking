using Application.ViewModel;
using MediatR;

namespace Application.Features.Payments.Commands
{
    public class HandleStripeWebhookCommand : IRequest<ResponseViewModel<bool>>
    {
        public string RawBody { get; set; } = string.Empty;

        public string StripeSignature { get; set; } = string.Empty;
    }
}