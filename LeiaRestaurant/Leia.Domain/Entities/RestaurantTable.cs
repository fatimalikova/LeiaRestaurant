using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class RestaurantTable : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Zone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}