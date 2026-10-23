using Leia.Application.Dtos.MenuItemDtos;

namespace Leia.Application.Dtos.MenuCategoryDtos
{
    public class ResponseMenuCategoryDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public List<ResponseMenuItemDto> Items { get; set; } = new();
    }
}