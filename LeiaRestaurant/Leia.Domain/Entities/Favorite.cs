using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class Favorite : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}