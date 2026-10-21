using Leia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Entities
{
    public class UnavailableDay : BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public DateTime Date { get; set; }
        public string? Reason { get; set; }
    }
}
