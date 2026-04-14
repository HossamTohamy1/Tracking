using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands
{
    public class AssignContainerCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid ImportRequestId { get; set; }
        public Guid ContainerId { get; set; }
    }
}
