using Leia.Domain.Common;
using Leia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;

        public Guid CustomerId { get; set; }
        public ApplicationUser Customer { get; set; } = null!;

        public Guid RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }

        public string? DeliveryAddress { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }

        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Review? Review { get; set; }
    }
}
