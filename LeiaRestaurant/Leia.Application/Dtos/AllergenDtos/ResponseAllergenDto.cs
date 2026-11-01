namespace Leia.Application.Dtos.AllergenDtos
{
    public class ResponseAllergenDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; }
    }
}