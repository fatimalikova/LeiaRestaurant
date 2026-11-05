using Leia.Application.Dtos.NotificationDtos;

namespace Leia.Application.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task SendNotificationAsync(Guid userId, ResponseNotificationDto notification);
    }
}