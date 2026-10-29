using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Dtos.RestaurantImageDtos;
using Leia.Application.Dtos.WorkingHourDtos;
using Leia.Domain.Enums;

namespace Leia.Application.Dtos.RestaurantDtos
{
    public class ResponseRestaurantDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public RestaurantStatus Status { get; set; }
        public bool IsActive { get; set; }
        public bool AcceptsDelivery { get; set; }
        public bool AcceptsPickup { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ResponseWorkingHourDto> WorkingHours { get; set; } = new();
        public List<ResponseCuisineDto> Cuisines { get; set; } = new();
        public List<ResponseRestaurantImageDto> Images { get; set; } = new();
    }
}