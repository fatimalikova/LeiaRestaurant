namespace Leia.Application.Dtos.RestaurantDtos
{
    public class RestaurantListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool AcceptsDelivery { get; set; }
        public bool AcceptsPickup { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsOpenNow { get; set; }
    }
}   