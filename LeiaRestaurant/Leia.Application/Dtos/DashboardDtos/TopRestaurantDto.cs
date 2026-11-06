namespace Leia.Application.Dtos.DashboardDtos
{
    public class TopRestaurantDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
    }
}