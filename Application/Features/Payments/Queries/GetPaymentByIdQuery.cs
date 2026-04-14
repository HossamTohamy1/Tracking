using Application.ViewModel;
using Application.DTOs.PaymentDtos;
using MediatR;

namespace Application.Features.Payments.Queries
{
    public record GetPaymentByIdQuery(Guid PaymentId)
        : IRequest<ResponseViewModel<PaymentResponseDto>>;
}