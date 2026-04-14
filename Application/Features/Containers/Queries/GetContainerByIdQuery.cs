using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Queries
{
    public class GetContainerByIdQuery : IRequest<ResponseViewModel<ContainerDto>>
    {
        public Guid ContainerId { get; set; }
    }
}
