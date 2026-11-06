namespace Leia.Application.Dtos.MenuItemDtos
{
    public class ResponseMenuSearchItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string? RestaurantLogoUrl { get; set; }
    }
}