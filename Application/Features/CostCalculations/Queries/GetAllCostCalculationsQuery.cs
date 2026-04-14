using Application.DTOs.CostCalculationDtos;
using Application.Features.Containers.Queries;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CostCalculations.Queries

{
    public record GetAllCostCalculationsQuery(
        int Page = 1,
        int PageSize = 10,
        string? Currency = null)
        : IRequest<ResponseViewModel<PaginatedResult<CostCalculationDto>>>;
}
