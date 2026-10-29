namespace Leia.Application.Dtos.CuisineDtos
{
    public class ResponseCuisineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}