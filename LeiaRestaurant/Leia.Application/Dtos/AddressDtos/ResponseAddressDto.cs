namespace Leia.Application.Dtos.AddressDtos
{
    public class ResponseAddressDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string? Note { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDefault { get; set; }
    }
}