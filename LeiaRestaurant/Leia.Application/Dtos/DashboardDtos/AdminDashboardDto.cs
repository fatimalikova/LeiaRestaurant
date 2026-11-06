using Leia.Domain.Enums;

namespace Leia.Application.Dtos.DashboardDtos
{
    public class AdminDashboardDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int TotalCustomers { get; set; }
        public int TotalRestaurantOwners { get; set; }
        public Dictionary<RestaurantStatus, int> RestaurantsByStatus { get; set; } = new();

        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int Reservations { get; set; }

        public List<DailyStatDto> Daily { get; set; } = new();
        public List<TopRestaurantDto> TopRestaurants { get; set; } = new();
    }
}