// Application/Features/Payments/Queries/Handlers/GetPaymentByIdQueryHandler.cs
using Application.DTOs.PaymentDtos;
using Application.ViewModel;
using AutoMapper;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Payments.Queries.Handlers
{
    public class GetPaymentByIdQueryHandler
        : IRequestHandler<GetPaymentByIdQuery, ResponseViewModel<PaymentResponseDto>>
    {
        private readonly IGeneralRepository<Payment> _paymentRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentByIdQueryHandler> _logger;

        public GetPaymentByIdQueryHandler(
            IGeneralRepository<Payment> paymentRepo,
            IMapper mapper,
            ILogger<GetPaymentByIdQueryHandler> logger)
        {
            _paymentRepo = paymentRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentResponseDto>> Handle(
            GetPaymentByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[GET PAYMENT BY ID] PaymentId: {PaymentId}", request.PaymentId);

            var payment = await _paymentRepo.GetByIdAsync(request.PaymentId);
            if (payment == null)
                throw new NotFoundException($"Payment with ID {request.PaymentId} not found", "Payments");

            var dto = _mapper.Map<PaymentResponseDto>(payment);
            return ResponseViewModel<PaymentResponseDto>.Success(dto);
        }
    }
}