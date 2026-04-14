using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands
{
    public class DeleteContainerCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid ContainerId { get; set; }
    }
}
