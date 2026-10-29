namespace Leia.Domain.Entities;

public class RestaurantCuisine
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public Guid CuisineId { get; set; }
    public Cuisine Cuisine { get; set; } = null!;
}