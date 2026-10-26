namespace Leia.Application.Dtos.SliderDtos
{
    public class ResponseSliderDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Link { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}