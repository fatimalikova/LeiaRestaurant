using AutoMapper;
using Leia.Application.Dtos.UnavailableDayDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class UnavailableDayService(AppDbContext db, IMapper mapper) : IUnavailableDayService
    {
        public async Task<List<ResponseUnavailableDayDto>> GetByRestaurantAsync(Guid restaurantId)
        {
            var today = TimeHelper.NowLocal().Date;

            var days = await db.UnavailableDays.AsNoTracking()
                .Where(u => u.RestaurantId == restaurantId && u.Date >= today)
                .OrderBy(u => u.Date)
                .ToListAsync();

            return mapper.Map<List<ResponseUnavailableDayDto>>(days);
        }

        public async Task<ResponseUnavailableDayDto> CreateAsync(Guid ownerId, CreateUnavailableDayDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var date = dto.Date.Date;

            if (await db.UnavailableDays.AnyAsync(u => u.RestaurantId == restaurant.Id && u.Date == date))
                throw new ConflictException("This day is already marked as unavailable.");

            var day = mapper.Map<UnavailableDay>(dto);
            day.RestaurantId = restaurant.Id;
            day.Date = date;

            db.UnavailableDays.Add(day);
            await db.SaveChangesAsync();
            return mapper.Map<ResponseUnavailableDayDto>(day);
        }

        public async Task DeleteAsync(Guid ownerId, Guid id)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var day = await db.UnavailableDays.FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Unavailable day not found.");

            db.UnavailableDays.Remove(day);
            await db.SaveChangesAsync();
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }
    }
}