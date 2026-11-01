namespace Leia.Application.Dtos.VariantDtos
{
    public class ResponseVariantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; }
        public int SortOrder { get; set; }
        public bool IsAvailable { get; set; }
    }
}