using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class ModifierOption : BaseEntity
{
    public Guid GroupId { get; set; }
    public ModifierGroup Group { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public int SortOrder { get; set; }
    public bool IsAvailable { get; set; } = true;
}