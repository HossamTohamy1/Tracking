using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands
{
    public class UpdateContainerStatusCommand : IRequest<ResponseViewModel<ContainerDto>>
    {
        public Guid ContainerId { get; set; }
        public int Status { get; set; } // ContainerStatus enum
    }
}
