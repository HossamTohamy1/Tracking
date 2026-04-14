using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands
{
    public class UpdateContainerShippingCostCommand : IRequest<ResponseViewModel<ContainerDto>>
    {
        public Guid ContainerId { get; set; }
        public decimal TotalShippingCost { get; set; }
    }
}
