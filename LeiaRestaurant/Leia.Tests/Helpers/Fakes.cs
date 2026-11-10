using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.NotificationDtos;
using Leia.Application.Dtos.PaymentDtos;
using Leia.Application.Interfaces;
using Leia.Domain.Enums;

namespace Leia.Tests.Helpers
{
    public class FakeNotificationService : INotificationService
    {
        public List<(Guid UserId, string Title, NotificationType Type)> Sent { get; } = new();

        public Task CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null)
        {
            Sent.Add((userId, title, type));
            return Task.CompletedTask;
        }

        public Task<PagedResultDto<ResponseNotificationDto>> GetMyAsync(Guid userId, PaginationDto dto) => throw new NotSupportedException();
        public Task<int> GetUnreadCountAsync(Guid userId) => throw new NotSupportedException();
        public Task MarkAsReadAsync(Guid userId, Guid id) => throw new NotSupportedException();
        public Task MarkAllAsReadAsync(Guid userId) => throw new NotSupportedException();
    }

    public class FakePaymentService : IPaymentService
    {
        public Task RefundIfPaidAsync(Guid orderId, string reason) => Task.CompletedTask;

        public Task<ResponsePaymentDto> PayAsync(Guid customerId, PayOrderDto dto) => throw new NotSupportedException();
        public Task<ResponsePaymentDto> GetByOrderAsync(Guid userId, Guid orderId) => throw new NotSupportedException();
        public Task<ResponsePaymentDto> RefundAsync(Guid orderId, string? reason) => throw new NotSupportedException();
    }
}