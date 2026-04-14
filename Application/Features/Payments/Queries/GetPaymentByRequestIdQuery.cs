using Application.ViewModel;
using Application.DTOs.PaymentDtos;
using MediatR;

namespace Application.Features.Payments.Queries
{
    public record GetPaymentByRequestIdQuery(Guid ImportRequestId)
        : IRequest<ResponseViewModel<List<PaymentResponseDto>>>;
}