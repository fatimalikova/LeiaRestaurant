using AutoMapper;
using Leia.Application.Dtos.TableDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class TableService(AppDbContext db, IMapper mapper) : ITableService
    {
        private const int MaxTables = 100;

        public async Task<List<ResponseTableDto>> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var tables = await db.RestaurantTables.AsNoTracking()
                .Where(t => t.RestaurantId == restaurant.Id)
                .OrderBy(t => t.Zone).ThenBy(t => t.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseTableDto>>(tables);
        }

        public async Task<ResponseTableDto> CreateAsync(Guid ownerId, CreateTableDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var name = dto.Name.Trim();

            if (await db.RestaurantTables.CountAsync(t => t.RestaurantId == restaurant.Id) >= MaxTables)
                throw new BadRequestException($"A restaurant can have at most {MaxTables} tables.");

            if (await db.RestaurantTables.AnyAsync(t => t.RestaurantId == restaurant.Id && t.Name == name))
                throw new ConflictException("A table with this name already exists.");

            var table = mapper.Map<RestaurantTable>(dto);
            table.RestaurantId = restaurant.Id;
            table.Name = name;

            db.RestaurantTables.Add(table);
            await db.SaveChangesAsync();
            return mapper.Map<ResponseTableDto>(table);
        }

        public async Task<ResponseTableDto> UpdateAsync(Guid ownerId, Guid id, UpdateTableDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var table = await db.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Table not found.");

            var name = dto.Name.Trim();

            if (await db.RestaurantTables.AnyAsync(t => t.RestaurantId == restaurant.Id && t.Name == name && t.Id != id))
                throw new ConflictException("A table with this name already exists.");

            if (!dto.IsActive && table.IsActive && await HasUpcomingReservationsAsync(id))
                throw new BadRequestException("This table has upcoming reservations and cannot be deactivated.");

            mapper.Map(dto, table);
            table.Name = name;
            table.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseTableDto>(table);
        }

        public async Task DeleteAsync(Guid ownerId, Guid id)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var table = await db.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Table not found.");

            if (await HasUpcomingReservationsAsync(id))
                throw new BadRequestException("This table has upcoming reservations and cannot be deleted.");

            if (await db.Reservations.AnyAsync(r => r.TableId == id))
                throw new BadRequestException("This table has reservation history. Deactivate it instead.");

            db.RestaurantTables.Remove(table);
            await db.SaveChangesAsync();
        }

        private async Task<bool> HasUpcomingReservationsAsync(Guid tableId)
        {
            var now = TimeHelper.NowLocal();
            var active = new[] { ReservationStatus.Pending, ReservationStatus.Confirmed };

            return await db.Reservations.AnyAsync(r =>
                r.TableId == tableId && active.Contains(r.Status) && r.EndDateTime > now);
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }
    }
}