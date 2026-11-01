using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class OrderItemModifier : BaseEntity
{
    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
}