// Application/Features/Payments/Queries/Handlers/GetPaymentByRequestIdQueryHandler.cs
using Application.DTOs.PaymentDtos;
using Application.ViewModel;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Payments.Queries.Handlers
{
    public class GetPaymentByRequestIdQueryHandler
        : IRequestHandler<GetPaymentByRequestIdQuery, ResponseViewModel<List<PaymentResponseDto>>>
    {
        private readonly IGeneralRepository<Payment> _paymentRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentByRequestIdQueryHandler> _logger;

        public GetPaymentByRequestIdQueryHandler(
            IGeneralRepository<Payment> paymentRepo,
            IMapper mapper,
            ILogger<GetPaymentByRequestIdQueryHandler> logger)
        {
            _paymentRepo = paymentRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<PaymentResponseDto>>> Handle(
            GetPaymentByRequestIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[GET PAYMENT BY REQUEST] RequestId: {RequestId}", request.ImportRequestId);

            var payments = await _paymentRepo.GetAll()
                .Where(p => p.ImportRequestId == request.ImportRequestId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<PaymentResponseDto>>(payments);
            return ResponseViewModel<List<PaymentResponseDto>>.Success(dtos);
        }
    }
}