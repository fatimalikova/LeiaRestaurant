using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid? MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    public string NameAtOrder { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public decimal PriceAtOrder { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }

    public ICollection<OrderItemModifier> Modifiers { get; set; } = new List<OrderItemModifier>();
}