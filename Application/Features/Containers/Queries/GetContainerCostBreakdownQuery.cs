using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Queries
{
    public class GetContainerCostBreakdownQuery : IRequest<ResponseViewModel<ContainerCostBreakdownDto>>
    {
        public Guid ContainerId { get; set; }
    }
}
