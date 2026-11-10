using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class MenuCategory : BaseEntity, ISoftDelete
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}