using Leia.Application.Dtos.ReservationDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Services;
using Leia.Tests.Helpers;
using Xunit;

namespace Leia.Tests
{
    public class ReservationServiceTests
    {
        private sealed class Ctx : IDisposable
        {
            public TestDb T { get; init; } = null!;
            public ReservationService Service { get; init; } = null!;
            public ApplicationUser Owner { get; init; } = null!;
            public ApplicationUser Alice { get; init; } = null!;
            public ApplicationUser Bob { get; init; } = null!;
            public Restaurant Restaurant { get; init; } = null!;

            public void Dispose() => T.Dispose();
        }

        private static Ctx Build(bool autoConfirm = false, params int[] tableCapacities)
        {
            var t = new TestDb();
            var owner = Seeder.AddUser(t.Db, "owner@test.com");
            var alice = Seeder.AddUser(t.Db, "alice@test.com");
            var bob = Seeder.AddUser(t.Db, "bob@test.com");
            var restaurant = Seeder.AddRestaurant(t.Db, owner.Id, new TimeSpan(10, 0, 0), new TimeSpan(23, 0, 0),
                reservations: true, autoConfirm: autoConfirm);

            var capacities = tableCapacities.Length == 0 ? new[] { 4 } : tableCapacities;
            for (var i = 0; i < capacities.Length; i++)
                t.Db.RestaurantTables.Add(new RestaurantTable { RestaurantId = restaurant.Id, Name = $"T{i + 1}", Capacity = capacities[i] });
            t.Db.SaveChanges();

            var service = new ReservationService(t.Db, new FakeNotificationService(), null!, t.Mapper);
            return new Ctx { T = t, Service = service, Owner = owner, Alice = alice, Bob = bob, Restaurant = restaurant };
        }

        private static DateTime Tomorrow(int hour, int minute = 0) =>
            TimeHelper.NowLocal().Date.AddDays(1).AddHours(hour).AddMinutes(minute);

        private static CreateReservationDto Reserve(Ctx c, DateTime start, int guests = 2) => new()
        {
            RestaurantId = c.Restaurant.Id,
            StartDateTime = start,
            GuestCount = guests,
            ContactPhone = "+994501234567"
        };

        [Fact]
        public async Task Reservation_is_pending_by_default()
        {
            using var c = Build();

            var result = await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));

            Assert.Equal(ReservationStatus.Pending, result.Status);
            Assert.Equal("T1", result.TableName);
            Assert.Equal(Tomorrow(13, 30), result.EndDateTime);
        }

        [Fact]
        public async Task Auto_confirm_confirms_immediately()
        {
            using var c = Build(autoConfirm: true);

            var result = await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));

            Assert.Equal(ReservationStatus.Confirmed, result.Status);
        }

        [Fact]
        public async Task Same_table_cannot_be_booked_twice_at_overlapping_times()
        {
            using var c = Build();

            await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Bob.Id, Reserve(c, Tomorrow(12))));

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Bob.Id, Reserve(c, Tomorrow(13))));
        }

        [Fact]
        public async Task Table_is_free_again_right_after_the_reservation_ends()
        {
            using var c = Build();

            await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));
            var result = await c.Service.CreateAsync(c.Bob.Id, Reserve(c, Tomorrow(13, 30)));

            Assert.Equal(Tomorrow(13, 30), result.StartDateTime);
        }

        [Fact]
        public async Task Smallest_suitable_table_is_chosen()
        {
            using var c = Build(false, 6, 2, 4);

            var result = await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12), guests: 2));

            Assert.Equal("T2", result.TableName);
        }

        [Fact]
        public async Task Party_larger_than_any_table_is_rejected()
        {
            using var c = Build(false, 4);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12), guests: 8)));
        }

        [Fact]
        public async Task Customer_cannot_hold_two_overlapping_reservations()
        {
            using var c = Build(false, 4, 4);

            await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(13))));
        }

        [Fact]
        public async Task Time_outside_working_hours_is_rejected()
        {
            using var c = Build();

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(8))));

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(22))));
        }

        [Fact]
        public async Task Time_not_matching_the_slot_grid_is_rejected()
        {
            using var c = Build();

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12, 10))));
        }

        [Fact]
        public async Task Restaurant_without_reservations_enabled_is_rejected()
        {
            using var c = Build();
            c.Restaurant.AcceptsReservations = false;
            await c.T.Db.SaveChangesAsync();

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12))));
        }

        [Fact]
        public async Task Availability_hides_overlapping_slots_after_a_booking()
        {
            using var c = Build();
            await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));

            var result = await c.Service.GetAvailabilityAsync(new AvailabilityQueryDto
            {
                RestaurantId = c.Restaurant.Id,
                Date = Tomorrow(0),
                Guests = 2
            });

            var starts = result.Slots.Select(s => s.StartDateTime).ToList();

            Assert.Contains(Tomorrow(10), starts);
            Assert.DoesNotContain(Tomorrow(11), starts);
            Assert.DoesNotContain(Tomorrow(12), starts);
            Assert.DoesNotContain(Tomorrow(13), starts);
            Assert.Contains(Tomorrow(13, 30), starts);
            Assert.Equal(90, result.DurationMinutes);
        }

        [Fact]
        public async Task Customer_can_cancel_but_only_their_own_reservation()
        {
            using var c = Build();
            var reservation = await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                c.Service.CancelAsync(c.Bob.Id, reservation.Id, new CancelReservationDto()));

            var cancelled = await c.Service.CancelAsync(c.Alice.Id, reservation.Id, new CancelReservationDto { Reason = "Plans changed" });

            Assert.Equal(ReservationStatus.Cancelled, cancelled.Status);

            var again = await c.Service.CreateAsync(c.Bob.Id, Reserve(c, Tomorrow(12)));
            Assert.Equal(ReservationStatus.Pending, again.Status);
        }

        [Fact]
        public async Task Owner_can_confirm_and_cannot_complete_a_future_reservation()
        {
            using var c = Build();
            var reservation = await c.Service.CreateAsync(c.Alice.Id, Reserve(c, Tomorrow(12)));

            var confirmed = await c.Service.UpdateStatusAsync(c.Owner.Id, reservation.Id,
                new UpdateReservationStatusDto { Status = ReservationStatus.Confirmed });

            Assert.Equal(ReservationStatus.Confirmed, confirmed.Status);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.UpdateStatusAsync(c.Owner.Id, reservation.Id,
                    new UpdateReservationStatusDto { Status = ReservationStatus.Completed }));
        }
    }
}