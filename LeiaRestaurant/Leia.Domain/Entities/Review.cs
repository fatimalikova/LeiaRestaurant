using Leia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public Guid ClientId { get; set; }
        public ApplicationUser Client { get; set; } = null!;

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int Rating { get; set; }          
        public string? Comment { get; set; }
        public ReviewReply? Reply { get; set; } 
    }
}
