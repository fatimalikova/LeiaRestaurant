using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class MenuItemVariant : BaseEntity
{
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public int SortOrder { get; set; }
    public bool IsAvailable { get; set; } = true;
}