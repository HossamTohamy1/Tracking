using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Queries
{
    public class GetAllContainersQuery : IRequest<ResponseViewModel<PaginatedResult<ContainerListItemDto>>>
    {
        public int? Status { get; set; }
        public int? ManagedByOfficeId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
