using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class Cuisine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RestaurantCuisine> Restaurants { get; set; } = new List<RestaurantCuisine>();
}   