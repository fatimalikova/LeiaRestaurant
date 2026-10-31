using Leia.Domain.Enums;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class ResponseReservationDto
    {
        public Guid Id { get; set; }

        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;

        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;

        public int GuestCount { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public ReservationStatus Status { get; set; }

        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}