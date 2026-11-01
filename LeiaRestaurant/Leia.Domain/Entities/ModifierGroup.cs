using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class ModifierGroup : BaseEntity
{
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int MinSelect { get; set; }
    public int MaxSelect { get; set; } = 1;
    public int SortOrder { get; set; }

    public ICollection<ModifierOption> Options { get; set; } = new List<ModifierOption>();
}