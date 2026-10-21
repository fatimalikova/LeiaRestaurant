using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Accepted = 1,
        Preparing = 2,
        Ready = 3,
        OutForDelivery = 4,
        Completed = 5,
        Rejected = 6,
        Cancelled = 7
    }
}
