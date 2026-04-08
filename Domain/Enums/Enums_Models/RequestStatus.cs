using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums.Enums_Models
{
    public enum RequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Processing = 3,
        Shipped = 4,
        Customs = 5,
        OutForDelivery = 6,
        Delivered = 7,
        Cancelled = 8
    }
}
