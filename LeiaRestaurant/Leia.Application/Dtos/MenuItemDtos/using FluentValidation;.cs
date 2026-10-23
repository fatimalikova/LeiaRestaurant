namespace Leia.Application.Dtos.MenuItemDtos
{
    public class ResponseMenuItemDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int PrepMinutes { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        public bool DeactivatedByAdmin { get; set; }
    }
}