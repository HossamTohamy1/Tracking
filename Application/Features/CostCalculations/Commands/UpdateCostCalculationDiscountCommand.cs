using Application.DTOs.CostCalculationDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CostCalculations.Commands
{
    public record UpdateCostCalculationDiscountCommand(
        Guid RequestId,
        UpdateDiscountRequest Dto)
        : IRequest<ResponseViewModel<CostCalculationDto>>;
}