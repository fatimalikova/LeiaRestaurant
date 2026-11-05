using AutoMapper;
using Leia.Application.Dtos.PaymentDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Application.Models;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leia.Infrastructure.Services
{
    public class PaymentService(
        AppDbContext db,
        IPaymentGateway gateway,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<PaymentService> logger) : IPaymentService
    {
        private const int MaxAttempts = 5;

        public async Task<ResponsePaymentDto> PayAsync(Guid customerId, PayOrderDto dto)
        {
            var order = await db.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId)
                ?? throw new NotFoundException("Order not found.");

            if (order.CustomerId != customerId)
                throw new ForbiddenException("You can only pay for your own orders.");

            if (order.PaymentMethod != PaymentMethod.Card)
                throw new BadRequestException("This order is not paid by card.");

            if (order.Status != OrderStatus.Pending)
                throw new BadRequestException("This order can no longer be paid.");

            if (order.PaymentStatus == PaymentStatus.Paid)
                throw new ConflictException("This order has already been paid.");

            if (order.PaymentStatus != PaymentStatus.Pending && order.PaymentStatus != PaymentStatus.Failed)
                throw new BadRequestException("This order cannot be paid.");

            var payment = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
            if (payment == null)
            {
                payment = new Payment { OrderId = order.Id, Amount = order.Total, Provider = "Fake" };
                db.Payments.Add(payment);
            }

            if (payment.AttemptCount >= MaxAttempts)
                throw new BadRequestException("Too many failed payment attempts for this order.");

            payment.AttemptCount++;

            var result = await gateway.ChargeAsync(
                new CardDetails
                {
                    CardNumber = dto.CardNumber,
                    ExpMonth = dto.ExpMonth,
                    ExpYear = dto.ExpYear,
                    Cvv = dto.Cvv,
                    CardHolder = dto.CardHolder
                },
                order.Total,
                $"{order.Id}-{payment.AttemptCount}");

            payment.Brand = result.Brand;
            payment.Last4 = result.Last4;
            payment.UpdatedAt = DateTime.UtcNow;

            if (result.Success)
            {
                payment.Status = PaymentStatus.Paid;
                payment.ProviderPaymentId = result.ProviderPaymentId;
                payment.PaidAt = DateTime.UtcNow;
                payment.FailureReason = null;
                order.PaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = result.FailureReason;
                order.PaymentStatus = PaymentStatus.Failed;
            }

            order.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            if (!result.Success)
                throw new BadRequestException($"Payment failed: {result.FailureReason}");

            var message = order.ScheduledFor.HasValue
                ? $"New scheduled order {order.OrderNumber} for {order.ScheduledFor:dd MMM HH:mm} - {order.Total:0.00} AZN."
                : $"New order {order.OrderNumber} - {order.Total:0.00} AZN.";

            await notificationService.CreateAsync(
                order.Restaurant.OwnerId, "New order", message,
                NotificationType.NewOrder, order.Id);

            await notificationService.CreateAsync(
                customerId, "Payment successful",
                $"Your payment of {order.Total:0.00} AZN for order {order.OrderNumber} was successful.",
                NotificationType.PaymentUpdated, order.Id);

            return mapper.Map<ResponsePaymentDto>(payment);
        }

        public async Task<ResponsePaymentDto> GetByOrderAsync(Guid userId, Guid orderId)
        {
            var payment = await db.Payments.AsNoTracking()
                .Include(p => p.Order).ThenInclude(o => o.Restaurant)
                .FirstOrDefaultAsync(p => p.OrderId == orderId)
                ?? throw new NotFoundException("Payment not found.");

            if (payment.Order.CustomerId != userId && payment.Order.Restaurant.OwnerId != userId)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null || !await userManager.IsInRoleAsync(user, "Admin"))
                    throw new ForbiddenException("You are not authorized to view this payment.");
            }

            return mapper.Map<ResponsePaymentDto>(payment);
        }

        public async Task<ResponsePaymentDto> RefundAsync(Guid orderId, string? reason)
        {
            var payment = await db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId)
                ?? throw new NotFoundException("Payment not found.");

            if (payment.Status != PaymentStatus.Paid)
                throw new BadRequestException("Only paid payments can be refunded.");

            if (!await TryRefundAsync(payment, reason ?? "Refunded by administrator."))
                throw new BadRequestException("Refund failed. Please try again.");

            return mapper.Map<ResponsePaymentDto>(payment);
        }

        public async Task RefundIfPaidAsync(Guid orderId, string reason)
        {
            var payment = await db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Paid);

            if (payment == null)
                return;

            await TryRefundAsync(payment, reason);
        }

        private async Task<bool> TryRefundAsync(Payment payment, string reason)
        {
            try
            {
                var result = await gateway.RefundAsync(payment.ProviderPaymentId!, payment.Amount);

                if (!result.Success)
                {
                    logger.LogError("Refund failed for order {OrderId}: {Reason}", payment.OrderId, result.FailureReason);
                    return false;
                }

                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Order.PaymentStatus = PaymentStatus.Refunded;
                await db.SaveChangesAsync();

                await notificationService.CreateAsync(
                    payment.Order.CustomerId, "Payment refunded",
                    $"{payment.Amount:0.00} AZN for order {payment.Order.OrderNumber} was refunded. {reason}",
                    NotificationType.PaymentUpdated, payment.OrderId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Refund error for order {OrderId}", payment.OrderId);
                return false;
            }
        }
    }
}