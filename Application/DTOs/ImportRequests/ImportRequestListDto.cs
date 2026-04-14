using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ImportRequests
{
    public class ImportRequestListDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShipmentType { get; set; } = string.Empty;
        public string? AssignedOfficeName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}