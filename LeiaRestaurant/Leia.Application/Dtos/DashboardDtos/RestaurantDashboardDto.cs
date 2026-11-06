using Leia.Domain.Enums;

namespace Leia.Application.Dtos.DashboardDtos
{
    public class RestaurantDashboardDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }

        public int PendingOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int PendingReservations { get; set; }
        public int TodayReservations { get; set; }

        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new();
        public List<DailyStatDto> Daily { get; set; } = new();
        public List<TopItemDto> TopItems { get; set; } = new();
    }
}