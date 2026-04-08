using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums.Enums_Models
{
    public enum ShipmentStage
    {
        Purchased = 0,
        Processing = 1,
        ReadyToShip = 2,
        Shipped = 3,
        InTransit = 4,
        ArrivedPort = 5,
        Customs = 6,
        OutForDelivery = 7,
        Delivered = 8,
        Exception = 9
    }
}
