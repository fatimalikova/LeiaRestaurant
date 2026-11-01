using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class MenuItem : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public MenuCategory Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int PrepMinutes { get; set; }

    public bool IsAvailable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool DeactivatedByAdmin { get; set; }

    public ICollection<MenuItemVariant> Variants { get; set; } = new List<MenuItemVariant>();
    public ICollection<ModifierGroup> ModifierGroups { get; set; } = new List<ModifierGroup>();
    public ICollection<MenuItemAllergen> Allergens { get; set; } = new List<MenuItemAllergen>();
}