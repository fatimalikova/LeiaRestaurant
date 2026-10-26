namespace Leia.Application.Dtos.WorkingHourDtos
{
    public class ResponseWorkingHourDto
    {
        public Guid Id { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}