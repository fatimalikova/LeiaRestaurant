using Leia.Application.Dtos.NotificationDtos;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Leia.Api.Hubs
{
    public class SignalRNotifier(IHubContext<NotificationHub> hubContext) : IRealtimeNotifier
    {
        public Task SendNotificationAsync(Guid userId, ResponseNotificationDto notification)
        {
            return hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }
    }
}