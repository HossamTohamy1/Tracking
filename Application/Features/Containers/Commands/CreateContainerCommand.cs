using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using Domain.Enums.Enums_Models;
using MediatR;

namespace Application.Features.Containers.Commands
{
    public class CreateContainerCommand : IRequest<ResponseViewModel<ContainerDto>>
    {
        public decimal MaxWeightKg { get; set; }
        public decimal MaxVolumeCbm { get; set; }
        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ExpectedArrival { get; set; }

        public ShipmentType ShipmentType { get; set; } = ShipmentType.LCL;
    }
}