using AutoMapper;
using Leia.Application.Dtos.CouponDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class CouponService(AppDbContext db, IMapper mapper) : ICouponService
    {
        public async Task<List<ResponseCouponDto>> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var coupons = await db.Coupons.AsNoTracking()
                .Where(c => c.RestaurantId == restaurant.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return mapper.Map<List<ResponseCouponDto>>(coupons);
        }

        public async Task<List<ResponseCouponDto>> GetAllForAdminAsync()
        {
            var coupons = await db.Coupons.AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return mapper.Map<List<ResponseCouponDto>>(coupons);
        }

        public async Task<ResponseCouponDto> CreateAsync(Guid userId, bool isAdmin, CreateCouponDto dto)
        {
            Guid? restaurantId = null;
            if (!isAdmin)
                restaurantId = (await GetRestaurantAsync(userId)).Id;

            var code = Normalize(dto.Code);

            if (await db.Coupons.AnyAsync(c => c.Code == code))
                throw new ConflictException("A coupon with this code already exists.");

            var coupon = mapper.Map<Coupon>(dto);
            coupon.Code = code;
            coupon.RestaurantId = restaurantId;
            coupon.StartsAt = DateTime.SpecifyKind(dto.StartsAt, DateTimeKind.Utc);
            coupon.EndsAt = DateTime.SpecifyKind(dto.EndsAt, DateTimeKind.Utc);

            db.Coupons.Add(coupon);
            await db.SaveChangesAsync();
            return mapper.Map<ResponseCouponDto>(coupon);
        }

        public async Task<ResponseCouponDto> UpdateAsync(Guid userId, bool isAdmin, Guid id, UpdateCouponDto dto)
        {
            var coupon = await FindAsync(userId, isAdmin, id);

            if (dto.UsageLimit.HasValue && dto.UsageLimit.Value < coupon.UsedCount)
                throw new BadRequestException("Usage limit cannot be lower than the number of times the coupon was already used.");

            mapper.Map(dto, coupon);
            coupon.StartsAt = DateTime.SpecifyKind(dto.StartsAt, DateTimeKind.Utc);
            coupon.EndsAt = DateTime.SpecifyKind(dto.EndsAt, DateTimeKind.Utc);
            coupon.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseCouponDto>(coupon);
        }

        public async Task DeleteAsync(Guid userId, bool isAdmin, Guid id)
        {
            var coupon = await FindAsync(userId, isAdmin, id);

            if (await db.CouponUsages.AnyAsync(u => u.CouponId == id))
                throw new BadRequestException("This coupon has been used. Deactivate it instead.");

            db.Coupons.Remove(coupon);
            await db.SaveChangesAsync();
        }

        public async Task<AppliedCouponDto> EvaluateAsync(Guid customerId, Guid restaurantId, string code, decimal subtotal)
        {
            var normalized = Normalize(code);

            var coupon = await db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == normalized)
                ?? throw new BadRequestException("Coupon code is invalid.");

            if (!coupon.IsActive)
                throw new BadRequestException("This coupon is no longer active.");

            if (coupon.RestaurantId.HasValue && coupon.RestaurantId.Value != restaurantId)
                throw new BadRequestException("This coupon is not valid for this restaurant.");

            var now = DateTime.UtcNow;

            if (now < coupon.StartsAt)
                throw new BadRequestException("This coupon is not active yet.");

            if (now > coupon.EndsAt)
                throw new BadRequestException("This coupon has expired.");

            if (subtotal < coupon.MinOrderAmount)
                throw new BadRequestException($"This coupon requires a minimum order of {coupon.MinOrderAmount:0.00} AZN.");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                throw new BadRequestException("This coupon has reached its usage limit.");

            if (coupon.PerUserLimit.HasValue)
            {
                var used = await db.CouponUsages.CountAsync(u => u.CouponId == coupon.Id && u.UserId == customerId);
                if (used >= coupon.PerUserLimit.Value)
                    throw new BadRequestException("You have already used this coupon the maximum number of times.");
            }

            var discount = coupon.Type == CouponType.Percent
                ? Math.Round(subtotal * coupon.Value / 100m, 2)
                : coupon.Value;

            if (coupon.MaxDiscount.HasValue)
                discount = Math.Min(discount, coupon.MaxDiscount.Value);

            discount = Math.Min(discount, subtotal);

            return new AppliedCouponDto
            {
                CouponId = coupon.Id,
                Code = coupon.Code,
                DiscountAmount = discount
            };
        }

        private static string Normalize(string code) => code.Trim().ToUpperInvariant();

        private async Task<Coupon> FindAsync(Guid userId, bool isAdmin, Guid id)
        {
            var query = db.Coupons.AsQueryable();

            if (!isAdmin)
            {
                var restaurant = await GetRestaurantAsync(userId);
                query = query.Where(c => c.RestaurantId == restaurant.Id);
            }

            return await query.FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Coupon not found.");
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }
    }
}