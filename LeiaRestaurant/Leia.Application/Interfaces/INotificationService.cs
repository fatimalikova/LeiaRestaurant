using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.NotificationDtos;
using Leia.Domain.Enums;

namespace Leia.Application.Interfaces
{
    public interface INotificationService
    {
        Task<PagedResultDto<ResponseNotificationDto>> GetMyAsync(Guid userId, PaginationDto dto);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid userId, Guid id);
        Task MarkAllAsReadAsync(Guid userId);
        Task CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null);
    }
}