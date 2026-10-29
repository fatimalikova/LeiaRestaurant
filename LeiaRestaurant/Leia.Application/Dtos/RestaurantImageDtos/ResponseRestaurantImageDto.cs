namespace Leia.Application.Dtos.RestaurantImageDtos
{
    public class ResponseRestaurantImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}