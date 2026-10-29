using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class RestaurantImage : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}