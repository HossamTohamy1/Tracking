using Application.DTOs.CostCalculationDtos;
using Application.ViewModel;
using AutoMapper;
using Domain.Enums.Enums_Models;
using Domain.Exceptions;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;    
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CostCalculations.Commands.Handlers
{
    public class UpdateCostCalculationCommandHandler : IRequestHandler<UpdateCostCalculationCommand, ResponseViewModel<CostCalculationDto>>
    {
        private readonly IGeneralRepository<CostCalculation> _costRepo;
        private readonly IGeneralRepository<Payment> _paymentRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCostCalculationCommandHandler> _logger;

        public UpdateCostCalculationCommandHandler(
            IGeneralRepository<CostCalculation> costRepo,
            IGeneralRepository<Payment> paymentRepo,
            IMapper mapper,
            ILogger<UpdateCostCalculationCommandHandler> logger)
        {
            _costRepo = costRepo ?? throw new ArgumentNullException(nameof(costRepo));
            _paymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseViewModel<CostCalculationDto>> Handle(
            UpdateCostCalculationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[UPDATE COST CALCULATION] Updating cost components. RequestId: {RequestId}",
                    request.RequestId);

                // ── FETCH ─────────────────────────────────────────────────────
                var cost = await _costRepo.GetAll()
                    .FirstOrDefaultAsync(x => x.ImportRequestId == request.RequestId, cancellationToken);

                if (cost is null)
                {
                    _logger.LogWarning(
                        "[UPDATE COST CALCULATION] Not found. RequestId: {RequestId}",
                        request.RequestId);

                    throw new NotFoundException(
                        $"Cost calculation for request {request.RequestId} not found",
                        "CostCalculation");
                }

                // ── LOCK CHECK: block if a Completed payment exists ───────────
                var hasCompletedPayment = await _paymentRepo.GetAll()
                    .AnyAsync(p => p.ImportRequestId == request.RequestId
                                   && p.Status == PaymentStatus.Completed,
                              cancellationToken);

                if (hasCompletedPayment)
                {
                    _logger.LogWarning(
                        "[UPDATE COST CALCULATION] Record is locked (payment completed). RequestId: {RequestId}",
                        request.RequestId);

                    throw new BusinessLogicException(
                        "Cost calculation cannot be modified after payment is completed",
                        "CostCalculation");
                }

                // ── APPLY UPDATES ─────────────────────────────────────────────
                var dto = request.Dto;

                cost.BaseShippingCost = dto.BaseShippingCost;
                cost.CustomsDuty = dto.CustomsDuty;
                cost.TaxAmount = dto.TaxAmount;
                cost.InsuranceCost = dto.InsuranceCost;
                cost.HandlingFee = dto.HandlingFee;
                cost.OtherFees = dto.OtherFees;

                if (!string.IsNullOrWhiteSpace(dto.Notes))
                    cost.Notes = dto.Notes;

                if (!string.IsNullOrWhiteSpace(dto.Currency))
                    cost.Currency = dto.Currency.Trim().ToUpper();

                // ── RECALCULATE TOTALS ────────────────────────────────────────
                cost.TotalBeforeDiscount = cost.BaseShippingCost
                                         + cost.CustomsDuty
                                         + cost.TaxAmount
                                         + cost.InsuranceCost
                                         + cost.HandlingFee
                                         + cost.OtherFees;

                cost.FinalAmount = cost.TotalBeforeDiscount - cost.DiscountAmount;
                cost.UpdatedAt = DateTime.UtcNow;

                await _costRepo.SaveAsync();

                _logger.LogInformation(
                    "[UPDATE COST CALCULATION] Updated successfully. RequestId: {RequestId}, FinalAmount: {FinalAmount}",
                    request.RequestId,
                    cost.FinalAmount);

                var resultDto = _mapper.Map<CostCalculationDto>(cost);
                return ResponseViewModel<CostCalculationDto>.Success(
                    resultDto,
                    "Cost calculation updated successfully");
            }
            catch (NotFoundException) { throw; }
            catch (BusinessLogicException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[UPDATE COST CALCULATION] Unexpected error. RequestId: {RequestId}",
                    request.RequestId);

                throw new BusinessLogicException(
                    "Failed to update cost calculation",
                    ex,
                    "CostCalculation");
            }
        }
    }
}
