using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Queries
{
    public class GetOfficeContainersQuery : IRequest<ResponseViewModel<PaginatedResult<ContainerListItemDto>>>
    {
        public int? Status { get; set; } // Optional: filter by status
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
