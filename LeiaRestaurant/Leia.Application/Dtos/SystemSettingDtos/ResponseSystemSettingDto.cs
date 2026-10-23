namespace Leia.Application.Dtos.SystemSettingDtos
{
    public class ResponseSystemSettingDto
    {
        public int MinCancellationMinutes { get; set; }
        public int RestaurantResponseMinutes { get; set; }
        public bool RequireRestaurantApproval { get; set; }
    }
}