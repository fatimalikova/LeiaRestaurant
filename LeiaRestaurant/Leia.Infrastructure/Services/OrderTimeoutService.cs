using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leia.Infrastructure.Services
{
    public class OrderTimeoutService(
        AppDbContext db,
        ICouponService couponService,
        IPaymentService paymentService,
        INotificationService notificationService,
        ILogger<OrderTimeoutService> logger) : IOrderTimeoutService
    {
        private const int PaymentWindowMinutes = 15;
        private const int BatchSize = 50;

        public async Task<int> CancelExpiredAsync()
        {
            var setting = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
            var responseMinutes = setting?.RestaurantResponseMinutes ?? 5;

            var now = DateTime.UtcNow;
            var responseCutoff = now.AddMinutes(-responseMinutes);
            var paymentCutoff = now.AddMinutes(-PaymentWindowMinutes);

            var candidates = await db.Orders.AsNoTracking()
                .Where(o => o.Status == OrderStatus.Pending && (
                    (o.PaymentStatus == PaymentStatus.NotRequired && o.PlacedAt < responseCutoff) ||
                    (o.PaymentStatus == PaymentStatus.Paid
                        && db.Payments.Any(p => p.OrderId == o.Id && p.PaidAt < responseCutoff)) ||
                    ((o.PaymentStatus == PaymentStatus.Pending || o.PaymentStatus == PaymentStatus.Failed)
                        && o.PlacedAt < paymentCutoff)))
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.CustomerId,
                    o.PaymentStatus,
                    OwnerId = o.Restaurant.OwnerId
                })
                .Take(BatchSize)
                .ToListAsync();

            var cancelled = 0;

            foreach (var c in candidates)
            {
                try
                {
                    var visibleToRestaurant = c.PaymentStatus == PaymentStatus.NotRequired || c.PaymentStatus == PaymentStatus.Paid;
                    var reason = visibleToRestaurant
                        ? "The restaurant did not respond in time."
                        : "Payment was not completed in time.";

                    var affected = await db.Orders
                        .Where(o => o.Id == c.Id && o.Status == OrderStatus.Pending)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(o => o.Status, OrderStatus.Cancelled)
                            .SetProperty(o => o.CancelReason, reason)
                            .SetProperty(o => o.UpdatedAt, DateTime.UtcNow));

                    if (affected == 0)
                        continue;

                    db.OrderStatusHistories.Add(new OrderStatusHistory
                    {
                        OrderId = c.Id,
                        FromStatus = OrderStatus.Pending,
                        ToStatus = OrderStatus.Cancelled,
                        Comment = reason
                    });
                    await db.SaveChangesAsync();

                    await couponService.ReleaseForOrderAsync(c.Id);
                    await paymentService.RefundIfPaidAsync(c.Id, reason);

                    await notificationService.CreateAsync(
                        c.CustomerId, "Order cancelled",
                        $"Order {c.OrderNumber} was cancelled automatically. {reason}",
                        NotificationType.OrderCancelled, c.Id);

                    if (visibleToRestaurant)
                        await notificationService.CreateAsync(
                            c.OwnerId, "Order expired",
                            $"Order {c.OrderNumber} was cancelled because it was not answered in time.",
                            NotificationType.OrderCancelled, c.Id);

                    cancelled++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to cancel expired order {OrderId}", c.Id);
                    db.ChangeTracker.Clear();
                }
            }

            return cancelled;
        }
    }
}