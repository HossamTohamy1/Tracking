using Application.DTOs.CostCalculationDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CostCalculations.Queries
{
    public record GetCostCalculationByRequestIdQuery(Guid RequestId)
        : IRequest<ResponseViewModel<CostCalculationDto>>;
}
