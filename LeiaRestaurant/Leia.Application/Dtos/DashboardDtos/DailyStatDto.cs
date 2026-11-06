namespace Leia.Application.Dtos.DashboardDtos
{
    public class DailyStatDto
    {
        public DateTime Date { get; set; }
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
    }
}