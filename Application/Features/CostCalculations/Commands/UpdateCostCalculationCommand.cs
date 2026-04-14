using Application.DTOs.CostCalculationDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CostCalculations.Commands
{
    public record UpdateCostCalculationCommand(
         Guid RequestId,
         UpdateCostCalculationRequest Dto)
         : IRequest<ResponseViewModel<CostCalculationDto>>;
}

