using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Leia.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}