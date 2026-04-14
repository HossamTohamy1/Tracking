using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PaymentDtos
{
    public class CreatePaymentIntentRequest
    {
        public Guid ImportRequestId { get; set; }
        public string? Currency { get; set; } = "USD";
    }
}
