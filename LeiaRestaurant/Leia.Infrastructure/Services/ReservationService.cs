using AutoMapper;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.ReservationDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Leia.Infrastructure.Services
{
    public class ReservationService(
        AppDbContext db,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        IMapper mapper) : IReservationService
    {
        private const int MaxAdvanceDays = 30;
        private const int StepMinutes = 30;
        private const int LeadMinutes = 30;
        private const int CancelNoticeMinutes = 60;

        private static readonly ReservationStatus[] ActiveStatuses =
            { ReservationStatus.Pending, ReservationStatus.Confirmed };

        public async Task<ResponseAvailabilityDto> GetAvailabilityAsync(AvailabilityQueryDto query)
        {
            var restaurant = await LoadBookableRestaurantAsync(query.RestaurantId);

            var now = TimeHelper.NowLocal();
            var date = query.Date.Date;

            if (date < now.Date || date > now.Date.AddDays(MaxAdvanceDays))
                throw new BadRequestException($"Date must be within the next {MaxAdvanceDays} days.");

            var duration = restaurant.ReservationDurationMinutes;
            var result = new ResponseAvailabilityDto { Date = date, DurationMinutes = duration };

            if (restaurant.UnavailableDays.Any(u => u.Date.Date == date))
                return result;

            var hour = restaurant.WorkingHours.FirstOrDefault(w => w.Day == date.DayOfWeek);
            if (hour == null)
                return result;

            var dayStart = date + hour.StartTime;
            var dayEnd = date + hour.EndTime;

            var tableIds = await db.RestaurantTables.AsNoTracking()
                .Where(t => t.RestaurantId == restaurant.Id && t.IsActive && t.Capacity >= query.Guests)
                .Select(t => t.Id)
                .ToListAsync();

            if (tableIds.Count == 0)
                return result;

            var active = ActiveStatuses;
            var reservations = await db.Reservations.AsNoTracking()
                .Where(r => r.RestaurantId == restaurant.Id
                    && active.Contains(r.Status)
                    && r.StartDateTime < dayEnd
                    && r.EndDateTime > dayStart)
                .Select(r => new { r.TableId, r.StartDateTime, r.EndDateTime })
                .ToListAsync();

            for (var slot = dayStart; slot.AddMinutes(duration) <= dayEnd; slot = slot.AddMinutes(StepMinutes))
            {
                if (slot < now.AddMinutes(LeadMinutes))
                    continue;

                var slotEnd = slot.AddMinutes(duration);
                var start = slot;

                var free = tableIds.Count(t => !reservations.Any(r =>
                    r.TableId == t && r.StartDateTime < slotEnd && r.EndDateTime > start));

                if (free > 0)
                    result.Slots.Add(new AvailabilitySlotDto
                    {
                        StartDateTime = slot,
                        EndDateTime = slotEnd,
                        AvailableTables = free
                    });
            }

            return result;
        }

        public async Task<ResponseReservationDto> CreateAsync(Guid customerId, CreateReservationDto dto)
        {
            var restaurant = await LoadBookableRestaurantAsync(dto.RestaurantId);

            var start = DateTime.SpecifyKind(dto.StartDateTime, DateTimeKind.Unspecified);
            var end = start.AddMinutes(restaurant.ReservationDurationMinutes);

            ValidateSlot(restaurant, start, end);
            await EnsureNoClientConflictAsync(customerId, start, end, null);

            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var table = await FindFreeTableAsync(restaurant.Id, dto.GuestCount, start, end, null)
                ?? throw new BadRequestException("No table is available for the selected time and guest count.");

            var reservation = new Reservation
            {
                CustomerId = customerId,
                RestaurantId = restaurant.Id,
                TableId = table.Id,
                GuestCount = dto.GuestCount,
                StartDateTime = start,
                EndDateTime = end,
                ContactPhone = dto.ContactPhone,
                Notes = dto.Notes,
                Status = restaurant.AutoConfirmReservations ? ReservationStatus.Confirmed : ReservationStatus.Pending
            };

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            await notificationService.CreateAsync(
                restaurant.OwnerId, "New reservation",
                $"New reservation for {dto.GuestCount} guests on {start:dd MMM yyyy HH:mm}.",
                NotificationType.ReservationCreated, reservation.Id);

            if (reservation.Status == ReservationStatus.Confirmed)
                await notificationService.CreateAsync(
                    customerId, "Reservation confirmed",
                    $"Your reservation at \"{restaurant.Name}\" on {start:dd MMM yyyy HH:mm} is confirmed.",
                    NotificationType.ReservationStatusChanged, reservation.Id);

            return await GetMappedAsync(reservation.Id);
        }

        public async Task<PagedResultDto<ResponseReservationDto>> GetMyAsync(Guid customerId, ReservationFilterDto filter)
        {
            return await PageAsync(Query().Where(r => r.CustomerId == customerId), filter);
        }

        public async Task<ResponseReservationDto> CancelAsync(Guid customerId, Guid id, CancelReservationDto dto)
        {
            var reservation = await db.Reservations
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException("Reservation not found.");

            if (reservation.CustomerId != customerId)
                throw new ForbiddenException("You can only cancel your own reservations.");

            if (!ActiveStatuses.Contains(reservation.Status))
                throw new BadRequestException("This reservation can no longer be cancelled.");

            EnsureNoticeRespected(reservation.StartDateTime);

            reservation.Status = ReservationStatus.Cancelled;
            reservation.CancelReason = dto.Reason;
            reservation.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await notificationService.CreateAsync(
                reservation.Restaurant.OwnerId, "Reservation cancelled",
                $"The reservation on {reservation.StartDateTime:dd MMM yyyy HH:mm} was cancelled by the customer.",
                NotificationType.ReservationCancelled, reservation.Id);

            return await GetMappedAsync(reservation.Id);
        }

        public async Task<ResponseReservationDto> RescheduleAsync(Guid customerId, Guid id, RescheduleReservationDto dto)
        {
            var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException("Reservation not found.");

            if (reservation.CustomerId != customerId)
                throw new ForbiddenException("You can only change your own reservations.");

            if (!ActiveStatuses.Contains(reservation.Status))
                throw new BadRequestException("This reservation can no longer be changed.");

            EnsureNoticeRespected(reservation.StartDateTime);

            var restaurant = await LoadBookableRestaurantAsync(reservation.RestaurantId);

            var guests = dto.GuestCount ?? reservation.GuestCount;
            var start = DateTime.SpecifyKind(dto.NewStartDateTime, DateTimeKind.Unspecified);
            var end = start.AddMinutes(restaurant.ReservationDurationMinutes);

            ValidateSlot(restaurant, start, end);
            await EnsureNoClientConflictAsync(customerId, start, end, reservation.Id);

            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var table = await FindFreeTableAsync(restaurant.Id, guests, start, end, reservation.Id)
                ?? throw new BadRequestException("No table is available for the selected time and guest count.");

            reservation.TableId = table.Id;
            reservation.GuestCount = guests;
            reservation.StartDateTime = start;
            reservation.EndDateTime = end;
            reservation.Status = restaurant.AutoConfirmReservations ? ReservationStatus.Confirmed : ReservationStatus.Pending;
            reservation.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            await notificationService.CreateAsync(
                restaurant.OwnerId, "Reservation changed",
                $"A reservation was moved to {start:dd MMM yyyy HH:mm} for {guests} guests.",
                NotificationType.ReservationCreated, reservation.Id);

            return await GetMappedAsync(reservation.Id);
        }

        public async Task<PagedResultDto<ResponseReservationDto>> GetRestaurantReservationsAsync(Guid ownerId, ReservationFilterDto filter)
        {
            var restaurant = await GetOwnRestaurantAsync(ownerId);
            return await PageAsync(Query().Where(r => r.RestaurantId == restaurant.Id), filter);
        }

        public async Task<ResponseReservationDto> UpdateStatusAsync(Guid ownerId, Guid id, UpdateReservationStatusDto dto)
        {
            var restaurant = await GetOwnRestaurantAsync(ownerId);

            var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Reservation not found.");

            if (!IsTransitionAllowed(reservation.Status, dto.Status))
                throw new BadRequestException($"Reservation cannot move from {reservation.Status} to {dto.Status}.");

            if (dto.Status is ReservationStatus.Completed or ReservationStatus.NoShow
                && reservation.StartDateTime > TimeHelper.NowLocal())
                throw new BadRequestException("This status can only be set after the reservation start time.");

            reservation.Status = dto.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == ReservationStatus.Rejected)
                reservation.CancelReason = dto.Comment;

            await db.SaveChangesAsync();

            var message = dto.Status switch
            {
                ReservationStatus.Confirmed => $"Your reservation at \"{restaurant.Name}\" on {reservation.StartDateTime:dd MMM yyyy HH:mm} is confirmed.",
                ReservationStatus.Rejected => $"Your reservation at \"{restaurant.Name}\" was declined. Reason: {dto.Comment}",
                ReservationStatus.Completed => $"Thank you for visiting \"{restaurant.Name}\".",
                _ => $"Your reservation at \"{restaurant.Name}\" was marked as no-show."
            };

            await notificationService.CreateAsync(
                reservation.CustomerId, "Reservation update", message,
                NotificationType.ReservationStatusChanged, reservation.Id);

            return await GetMappedAsync(reservation.Id);
        }

        public async Task<ResponseReservationDto> GetByIdAsync(Guid userId, Guid id)
        {
            var reservation = await Query().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException("Reservation not found.");

            if (reservation.CustomerId != userId && reservation.Restaurant.OwnerId != userId)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null || !await userManager.IsInRoleAsync(user, "Admin"))
                    throw new ForbiddenException("You are not authorized to view this reservation.");
            }

            return mapper.Map<ResponseReservationDto>(reservation);
        }

        public async Task<PagedResultDto<ResponseReservationDto>> GetAllForAdminAsync(ReservationFilterDto filter)
        {
            return await PageAsync(Query(), filter);
        }

        public async Task<ReservationSettingsDto> GetSettingsAsync(Guid ownerId)
        {
            var restaurant = await GetOwnRestaurantAsync(ownerId);
            return ToSettings(restaurant);
        }

        public async Task<ReservationSettingsDto> UpdateSettingsAsync(Guid ownerId, ReservationSettingsDto dto)
        {
            var restaurant = await GetOwnRestaurantAsync(ownerId);

            if (dto.AcceptsReservations && !await db.RestaurantTables.AnyAsync(t => t.RestaurantId == restaurant.Id && t.IsActive))
                throw new BadRequestException("Add at least one active table before enabling reservations.");

            restaurant.AcceptsReservations = dto.AcceptsReservations;
            restaurant.AutoConfirmReservations = dto.AutoConfirmReservations;
            restaurant.ReservationDurationMinutes = dto.ReservationDurationMinutes;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return ToSettings(restaurant);
        }

        private static ReservationSettingsDto ToSettings(Restaurant r) => new()
        {
            AcceptsReservations = r.AcceptsReservations,
            AutoConfirmReservations = r.AutoConfirmReservations,
            ReservationDurationMinutes = r.ReservationDurationMinutes
        };

        private async Task<Restaurant> LoadBookableRestaurantAsync(Guid restaurantId)
        {
            var restaurant = await db.Restaurants.AsNoTracking()
                .Include(r => r.WorkingHours)
                .Include(r => r.UnavailableDays)
                .FirstOrDefaultAsync(r => r.Id == restaurantId)
                ?? throw new NotFoundException("Restaurant not found.");

            if (restaurant.Status != RestaurantStatus.Approved || !restaurant.IsActive)
                throw new BadRequestException("This restaurant is not currently available.");

            if (!restaurant.AcceptsReservations)
                throw new BadRequestException("This restaurant does not accept reservations.");

            return restaurant;
        }

        private static void ValidateSlot(Restaurant restaurant, DateTime start, DateTime end)
        {
            var now = TimeHelper.NowLocal();

            if (start < now.AddMinutes(LeadMinutes))
                throw new BadRequestException($"Reservations must be made at least {LeadMinutes} minutes in advance.");

            if (start.Date > now.Date.AddDays(MaxAdvanceDays))
                throw new BadRequestException($"Reservations can only be made up to {MaxAdvanceDays} days in advance.");

            if (restaurant.UnavailableDays.Any(u => u.Date.Date == start.Date))
                throw new BadRequestException("The restaurant is closed on this day.");

            var hour = restaurant.WorkingHours.FirstOrDefault(w => w.Day == start.DayOfWeek)
                ?? throw new BadRequestException("The restaurant is closed on this day.");

            if (end.Date != start.Date || start.TimeOfDay < hour.StartTime || end.TimeOfDay > hour.EndTime)
                throw new BadRequestException("The selected time is outside the restaurant's working hours.");

            var offset = start.TimeOfDay - hour.StartTime;
            if (start.Second != 0 || offset.TotalMinutes % StepMinutes != 0)
                throw new BadRequestException("Start time must match one of the available slots.");
        }

        private async Task EnsureNoClientConflictAsync(Guid customerId, DateTime start, DateTime end, Guid? excludeId)
        {
            var active = ActiveStatuses;

            var conflict = await db.Reservations.AnyAsync(r =>
                r.CustomerId == customerId
                && r.Id != excludeId
                && active.Contains(r.Status)
                && r.StartDateTime < end
                && r.EndDateTime > start);

            if (conflict)
                throw new BadRequestException("You already have a reservation at this time.");
        }

        private async Task<RestaurantTable?> FindFreeTableAsync(Guid restaurantId, int guests, DateTime start, DateTime end, Guid? excludeId)
        {
            var active = ActiveStatuses;

            return await db.RestaurantTables
                .Where(t => t.RestaurantId == restaurantId
                    && t.IsActive
                    && t.Capacity >= guests
                    && !db.Reservations.Any(r =>
                        r.TableId == t.Id
                        && r.Id != excludeId
                        && active.Contains(r.Status)
                        && r.StartDateTime < end
                        && r.EndDateTime > start))
                .OrderBy(t => t.Capacity).ThenBy(t => t.Name)
                .FirstOrDefaultAsync();
        }

        private static void EnsureNoticeRespected(DateTime start)
        {
            if ((start - TimeHelper.NowLocal()).TotalMinutes < CancelNoticeMinutes)
                throw new BadRequestException($"This action requires at least {CancelNoticeMinutes} minutes notice before the reservation.");
        }

        private static bool IsTransitionAllowed(ReservationStatus current, ReservationStatus next) => (current, next) switch
        {
            (ReservationStatus.Pending, ReservationStatus.Confirmed) => true,
            (ReservationStatus.Pending, ReservationStatus.Rejected) => true,
            (ReservationStatus.Confirmed, ReservationStatus.Rejected) => true,
            (ReservationStatus.Confirmed, ReservationStatus.Completed) => true,
            (ReservationStatus.Confirmed, ReservationStatus.NoShow) => true,
            _ => false
        };

        private async Task<Restaurant> GetOwnRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }

        private IQueryable<Reservation> Query() => db.Reservations
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Restaurant)
            .Include(r => r.Table);

        private async Task<ResponseReservationDto> GetMappedAsync(Guid id)
        {
            var reservation = await Query().FirstAsync(r => r.Id == id);
            return mapper.Map<ResponseReservationDto>(reservation);
        }

        private async Task<PagedResultDto<ResponseReservationDto>> PageAsync(IQueryable<Reservation> query, ReservationFilterDto f)
        {
            if (f.Status is ReservationStatus status)
                query = query.Where(r => r.Status == status);

            if (f.From is DateTime from)
                query = query.Where(r => r.StartDateTime >= from.Date);

            if (f.To is DateTime to)
            {
                var end = to.Date.AddDays(1);
                query = query.Where(r => r.StartDateTime < end);
            }

            var total = await query.CountAsync();
            var reservations = await query
                .OrderByDescending(r => r.StartDateTime)
                .Skip((f.Page - 1) * f.PageSize)
                .Take(f.PageSize)
                .ToListAsync();

            return new PagedResultDto<ResponseReservationDto>
            {
                Items = mapper.Map<List<ResponseReservationDto>>(reservations),
                TotalCount = total,
                Page = f.Page,
                PageSize = f.PageSize
            };
        }
    }
}