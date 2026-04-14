using Application.DTOs.CostCalculationDtos;
using Application.Features.Containers.Queries;
using Application.ViewModel;
using MediatR;

namespace Application.Features.CostCalculations.Queries
{

    public record GetMyCostCalculationsQuery(
        int Page = 1,
        int PageSize = 10,
        string? Currency = null)
        : IRequest<ResponseViewModel<PaginatedResult<CostCalculationDto>>>;
}