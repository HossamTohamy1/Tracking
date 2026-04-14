using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ImportRequests
{
    public class ApproveImportRequestDto
    {
        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateTime? ExpectedArrival { get; set; }
    }
}
