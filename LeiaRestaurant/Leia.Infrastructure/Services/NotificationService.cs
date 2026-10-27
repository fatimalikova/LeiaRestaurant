using AutoMapper;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.NotificationDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class NotificationService(AppDbContext db, IMapper mapper) : INotificationService
    {
        public async Task<PagedResultDto<ResponseNotificationDto>> GetMyAsync(Guid userId, PaginationDto dto)
        {
            var query = db.Notifications.AsNoTracking().Where(n => n.UserId == userId);

            var total = await query.CountAsync();
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            return new PagedResultDto<ResponseNotificationDto>
            {
                Items = mapper.Map<List<ResponseNotificationDto>>(notifications),
                TotalCount = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(Guid userId, Guid id)
        {
            var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId)
                ?? throw new NotFoundException("Notification not found.");

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null)
        {
            db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId
            });
            await db.SaveChangesAsync();
        }
    }
}