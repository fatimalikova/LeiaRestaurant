namespace Leia.Application.Dtos.ReviewDtos
{
    public class ResponseReviewDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientImageUrl { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}