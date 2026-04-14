using Domain.Enums.Enums_Models;
using System;
using System.Text.Json.Serialization;

namespace Application.DTOs.ImportRequests
{
    public class SubmitImportRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }
        public DateTime? RequestedDeliveryDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ShipmentType ShipmentType { get; set; } = ShipmentType.FullContainer;
    }
}