using Leia.Application.Dtos.WorkingHourDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class WorkingHourService(AppDbContext db) : IWorkingHourService
    {
        public async Task<List<ResponseWorkingHourDto>> GetByRestaurantAsync(Guid restaurantId)
        {
            var hours = await db.WorkingHours.AsNoTracking()
                .Where(w => w.RestaurantId == restaurantId)
                .ToListAsync();

            return hours.OrderBy(w => ((int)w.Day + 6) % 7).Select(Map).ToList();
        }

        public async Task<ResponseWorkingHourDto> CreateAsync(Guid ownerId, CreateWorkingHourDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            if (await db.WorkingHours.AnyAsync(w => w.RestaurantId == restaurant.Id && w.Day == dto.Day))
                throw new ConflictException($"Working hours for {dto.Day} already exist. Update them instead.");

            var hour = new WorkingHour
            {
                RestaurantId = restaurant.Id,
                Day = dto.Day,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            db.WorkingHours.Add(hour);
            await db.SaveChangesAsync();
            return Map(hour);
        }

        public async Task<ResponseWorkingHourDto> UpdateAsync(Guid ownerId, Guid id, UpdateWorkingHourDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var hour = await db.WorkingHours.FirstOrDefaultAsync(w => w.Id == id && w.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Working hour not found.");

            hour.StartTime = dto.StartTime;
            hour.EndTime = dto.EndTime;
            hour.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Map(hour);
        }

        public async Task DeleteAsync(Guid ownerId, Guid id)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var hour = await db.WorkingHours.FirstOrDefaultAsync(w => w.Id == id && w.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Working hour not found.");

            db.WorkingHours.Remove(hour);
            await db.SaveChangesAsync();
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }

        private static ResponseWorkingHourDto Map(WorkingHour w) => new()
        {
            Id = w.Id,
            Day = w.Day,
            StartTime = w.StartTime,
            EndTime = w.EndTime
        };
    }
}