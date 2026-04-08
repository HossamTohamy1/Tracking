using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums.Enums_Models
{
    public enum PaymentMethod
    {
        CreditCard = 0,
        DebitCard = 1,
        PayPal = 2,
        BankTransfer = 3,
        Stripe = 4,
        Cash = 5
    }
}
