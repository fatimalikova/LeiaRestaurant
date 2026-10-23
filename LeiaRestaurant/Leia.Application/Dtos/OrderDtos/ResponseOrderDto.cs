using Leia.Domain.Enums;

namespace Leia.Application.Dtos.OrderDtos
{
    public class ResponseOrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;

        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }

        public string? DeliveryAddress { get; set; }
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
        public DateTime PlacedAt { get; set; }

        public bool HasReview { get; set; }
        public List<ResponseOrderItemDto> Items { get; set; } = new();
    }

    public class ResponseOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid? MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }
}