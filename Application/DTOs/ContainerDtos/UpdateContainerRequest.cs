using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ContainerDtos
{
    public class UpdateContainerRequest
    {
        public string? ContainerNumber { get; set; }
        public decimal? MaxWeightKg { get; set; }
        public decimal? MaxVolumeCbm { get; set; }
        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ExpectedArrival { get; set; }
    }
}
