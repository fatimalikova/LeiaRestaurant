using Leia.Domain.Common;
using Leia.Domain.Enums;

namespace Leia.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public string? Comment { get; set; }
}