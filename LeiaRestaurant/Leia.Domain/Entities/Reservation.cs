using Leia.Domain.Common;
using Leia.Domain.Enums;

namespace Leia.Domain.Entities;

public class Reservation : BaseEntity
{
    public Guid CustomerId { get; set; }
    public ApplicationUser Customer { get; set; } = null!;

    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public Guid TableId { get; set; }
    public RestaurantTable Table { get; set; } = null!;

    public int GuestCount { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public string ContactPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancelReason { get; set; }
}