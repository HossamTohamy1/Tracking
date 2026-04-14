using System;

namespace Application.DTOs.ImportRequests
{
    public class UpdateStageDto
    {
     
        public string Stage { get; set; } = string.Empty;

        public string? Location { get; set; }
        public string? Notes { get; set; }
        public string? TrackingNumber { get; set; }
        public string? CarrierName { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}