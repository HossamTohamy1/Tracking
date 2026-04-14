using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Commands
{
    public class UpdateContainerCommand : IRequest<ResponseViewModel<ContainerDto>>
    {
        public Guid Id { get; set; }
        public string? ContainerNumber { get; set; }
        public decimal? MaxWeightKg { get; set; }
        public decimal? MaxVolumeCbm { get; set; }
        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ExpectedArrival { get; set; }
    }
}
