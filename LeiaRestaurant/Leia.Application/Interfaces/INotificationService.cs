using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.NotificationDtos;
using Leia.Application.Models;
using Leia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface INotificationService
    {
        Task<ResponseModel<PagedResultDto<ResponseNotificationDto>>> GetMyAsync(Guid userId, PaginationDto dto);
        Task<ResponseModel<int>> GetUnreadCountAsync(Guid userId);
        Task<ResponseModel<bool>> MarkAsReadAsync(Guid userId, Guid id);
        Task<ResponseModel<bool>> MarkAllAsReadAsync(Guid userId);

        Task CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null);
    }
}
