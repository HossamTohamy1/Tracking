using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums.Enums_Models
{
    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4,
        Cancelled = 5
    }
}
