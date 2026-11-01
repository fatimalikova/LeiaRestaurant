using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class Allergen : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItemAllergen> MenuItems { get; set; } = new List<MenuItemAllergen>();
}