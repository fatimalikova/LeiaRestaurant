using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Enums
{
    public enum NotificationType
    {
        NewOrder = 0,
        OrderStatusChanged = 1,
        OrderCancelled = 2,
        NewReview = 3,
        System = 4
    }
}
