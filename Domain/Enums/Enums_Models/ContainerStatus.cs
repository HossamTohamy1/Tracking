using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums.Enums_Models
{
    public enum ContainerStatus
    {
        Open = 0,          
        Closed = 1,       
        Shipped = 2,       
        InTransit = 3,
        ArrivedPort = 4,
        Customs = 5,
        Delivered = 6,
        Cancelled = 7
    }
}
