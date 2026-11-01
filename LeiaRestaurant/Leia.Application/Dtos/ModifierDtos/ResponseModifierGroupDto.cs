namespace Leia.Application.Dtos.ModifierDtos
{
    public class ResponseModifierGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MinSelect { get; set; }
        public int MaxSelect { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public List<ResponseModifierOptionDto> Options { get; set; } = new();
    }

    public class ResponseModifierOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; }
        public int SortOrder { get; set; }
        public bool IsAvailable { get; set; }
    }
}