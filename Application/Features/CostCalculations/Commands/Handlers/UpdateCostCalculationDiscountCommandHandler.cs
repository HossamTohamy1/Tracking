using Application.DTOs.CostCalculationDtos;
using Application.ViewModel;
using AutoMapper;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CostCalculations.Commands.Handlers
{
    public class UpdateCostCalculationDiscountCommandHandler
           : IRequestHandler<UpdateCostCalculationDiscountCommand, ResponseViewModel<CostCalculationDto>>
    {
        private readonly IGeneralRepository<CostCalculation> _costRepo;
        private readonly IGeneralRepository<Payment> _paymentRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCostCalculationDiscountCommandHandler> _logger;

        public UpdateCostCalculationDiscountCommandHandler(
            IGeneralRepository<CostCalculation> costRepo,
            IGeneralRepository<Payment> paymentRepo,
            IMapper mapper,
            ILogger<UpdateCostCalculationDiscountCommandHandler> logger)
        {
            _costRepo = costRepo ?? throw new ArgumentNullException(nameof(costRepo));
            _paymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<CostCalculationDto>> Handle(
            UpdateCostCalculationDiscountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE DISCOUNT] Applying discount. RequestId: {RequestId}, Discount: {Discount}",
                    request.RequestId,
                    request.Dto.DiscountAmount);

                // ── FETCH ─────────────────────────────────────────────────────
                var cost = await _costRepo.GetAll()
                    .FirstOrDefaultAsync(x => x.ImportRequestId == request.RequestId, cancellationToken);

                if (cost is null)
                {
                    _logger.LogWarning(
                        "[UPDATE DISCOUNT] Not found. RequestId: {RequestId}",
                        request.RequestId);

                    throw new NotFoundException(
                        $"Cost calculation for request {request.RequestId} not found",
                        "CostCalculation");
                }

                // ── LOCK CHECK ────────────────────────────────────────────────
                var hasCompletedPayment = await _paymentRepo.GetAll()
                    .AnyAsync(p => p.ImportRequestId == request.RequestId
                                   && p.Status == PaymentStatus.Completed,
                              cancellationToken);

                if (hasCompletedPayment)
                {
                    _logger.LogWarning(
                        "[UPDATE DISCOUNT] Record is locked. RequestId: {RequestId}",
                        request.RequestId);

                    throw new BusinessLogicException(
                        "Discount cannot be changed after payment is completed",
                        "CostCalculation");
                }

                // ── VALIDATE DISCOUNT ─────────────────────────────────────────
                if (request.Dto.DiscountAmount < 0)
                {
                    throw new BusinessLogicException(
                        "Discount amount cannot be negative",
                        "CostCalculation");
                }

                if (request.Dto.DiscountAmount > cost.TotalBeforeDiscount)
                {
                    throw new BusinessLogicException(
                        "Discount amount cannot exceed the total before discount",
                        "CostCalculation");
                }

                // ── APPLY ─────────────────────────────────────────────────────
                cost.DiscountAmount = request.Dto.DiscountAmount;
                cost.FinalAmount = cost.TotalBeforeDiscount - cost.DiscountAmount;
                cost.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(request.Dto.Notes))
                    cost.Notes = request.Dto.Notes;

                await _costRepo.SaveAsync();

                _logger.LogInformation(
                    "[UPDATE DISCOUNT] Discount applied. RequestId: {RequestId}, FinalAmount: {FinalAmount}",
                    request.RequestId,
                    cost.FinalAmount);

                var resultDto = _mapper.Map<CostCalculationDto>(cost);
                return ResponseViewModel<CostCalculationDto>.Success(
                    resultDto,
                    "Discount applied successfully");
            }
            catch (NotFoundException) { throw; }
            catch (BusinessLogicException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[UPDATE DISCOUNT] Unexpected error. RequestId: {RequestId}",
                    request.RequestId);

                throw new BusinessLogicException(
                    "Failed to update discount",
                    ex,
                    "CostCalculation");
            }
        }
    }
}
