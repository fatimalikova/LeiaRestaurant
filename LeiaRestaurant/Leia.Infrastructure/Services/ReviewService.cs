using AutoMapper;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.ReviewDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class ReviewService(AppDbContext db, INotificationService notificationService, IMapper mapper) : IReviewService
    {
        public async Task<ResponseReviewDto> CreateAsync(Guid clientId, CreateReviewDto dto)
        {
            var order = await db.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId)
                ?? throw new NotFoundException("Order not found.");

            if (order.CustomerId != clientId)
                throw new ForbiddenException("You can only review your own orders.");

            if (order.Status != OrderStatus.Completed)
                throw new BadRequestException("Only completed orders can be reviewed.");

            if (await db.Reviews.AnyAsync(r => r.OrderId == order.Id))
                throw new ConflictException("This order has already been reviewed.");

            var review = mapper.Map<Review>(dto);
            review.RestaurantId = order.RestaurantId;
            review.ClientId = clientId;

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            await notificationService.CreateAsync(
                order.Restaurant.OwnerId, "New review",
                $"You received a {dto.Rating}-star review.",
                NotificationType.NewReview, review.Id);

            var client = await db.Users.AsNoTracking().FirstAsync(u => u.Id == clientId);

            var result = mapper.Map<ResponseReviewDto>(review);
            result.ClientName = client.FullName;
            result.ClientImageUrl = client.ImageUrl;
            return result;
        }

        public async Task<PagedResultDto<ResponseReviewDto>> GetByRestaurantAsync(Guid restaurantId, PaginationDto dto)
        {
            var query = db.Reviews.AsNoTracking()
                .Include(r => r.Client)
                .Include(r => r.Reply)
                .Where(r => r.RestaurantId == restaurantId);

            var total = await query.CountAsync();
            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            return new PagedResultDto<ResponseReviewDto>
            {
                Items = mapper.Map<List<ResponseReviewDto>>(reviews),
                TotalCount = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }

        public async Task<ResponseReviewDto> ReplyAsync(Guid ownerId, Guid reviewId, CreateReviewReplyDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var review = await db.Reviews
                .Include(r => r.Client)
                .Include(r => r.Reply)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Review not found.");

            if (review.Reply != null)
                throw new ConflictException("You have already replied to this review.");

            review.Reply = new ReviewReply
            {
                OwnerId = ownerId,
                Content = dto.Content.Trim()
            };

            await db.SaveChangesAsync();

            await notificationService.CreateAsync(
                review.ClientId, "Restaurant replied to your review",
                $"\"{restaurant.Name}\" replied to your review.",
                NotificationType.ReviewReplied, review.Id);

            return mapper.Map<ResponseReviewDto>(review);
        }

        public async Task DeleteReplyAsync(Guid ownerId, Guid reviewId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var review = await db.Reviews
                .Include(r => r.Reply)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Review not found.");

            if (review.Reply == null)
                throw new NotFoundException("Reply not found.");

            db.ReviewReplies.Remove(review.Reply);
            await db.SaveChangesAsync();
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }
    }
}