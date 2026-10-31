namespace Leia.Application.Dtos.ReservationDtos
{
    public class ResponseAvailabilityDto
    {
        public DateTime Date { get; set; }
        public int DurationMinutes { get; set; }
        public List<AvailabilitySlotDto> Slots { get; set; } = new();
    }

    public class AvailabilitySlotDto
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int AvailableTables { get; set; }
    }
}