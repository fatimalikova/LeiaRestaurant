using Leia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Entities
{
    public class SystemSetting : BaseEntity
    {
        public int MinCancellationMinutes { get; set; } = 5;
        public int RestaurantResponseMinutes { get; set; } = 5;
        public bool RequireRestaurantApproval { get; set; } = true;
    }
}
