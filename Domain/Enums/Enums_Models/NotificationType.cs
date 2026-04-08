using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums.Enums_Models
{
    public enum NotificationType
    {
        ShipmentUpdate = 0,
        PaymentSuccess = 1,
        PaymentFailed = 2,
        NewMessage = 3,
        CustomsUpdate = 4,
        RequestApproved = 5,
        RequestRejected = 6,
        ContainerUpdate = 7,
        General = 8
    }
}
