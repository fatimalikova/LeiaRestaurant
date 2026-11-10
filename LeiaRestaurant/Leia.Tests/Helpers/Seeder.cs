using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;

namespace Leia.Tests.Helpers
{
    public static class Seeder
    {
        public static readonly TimeSpan DayStart = TimeSpan.Zero;
        public static readonly TimeSpan DayEnd = new(23, 59, 59);

        public static ApplicationUser AddUser(AppDbContext db, string email)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                FullName = email
            };

            db.Users.Add(user);
            db.SaveChanges();
            return user;
        }

        public static Restaurant AddRestaurant(
            AppDbContext db,
            Guid ownerId,
            TimeSpan open,
            TimeSpan close,
            bool reservations = false,
            bool autoConfirm = false)
        {
            var restaurant = new Restaurant
            {
                OwnerId = ownerId,
                Name = "Test Restaurant",
                Address = "Baku",
                PhoneNumber = "+994501234567",
                Status = RestaurantStatus.Approved,
                AcceptsReservations = reservations,
                AutoConfirmReservations = autoConfirm
            };

            foreach (var day in Enum.GetValues<DayOfWeek>())
                restaurant.WorkingHours.Add(new WorkingHour { Day = day, StartTime = open, EndTime = close });

            db.Restaurants.Add(restaurant);
            db.SaveChanges();
            return restaurant;
        }

        public static MenuItem AddItem(AppDbContext db, Restaurant restaurant, decimal price, string name = "Pizza")
        {
            var category = new MenuCategory { RestaurantId = restaurant.Id, Name = "Main" };
            db.MenuCategories.Add(category);
            db.SaveChanges();

            var item = new MenuItem
            {
                RestaurantId = restaurant.Id,
                CategoryId = category.Id,
                Name = name,
                Price = price
            };

            db.MenuItems.Add(item);
            db.SaveChanges();
            return item;
        }

        public static Coupon AddCoupon(
            AppDbContext db,
            Guid? restaurantId,
            string code = "WELCOME10",
            CouponType type = CouponType.Percent,
            decimal value = 10,
            decimal minOrder = 0,
            decimal? maxDiscount = null,
            int? usageLimit = null,
            int? perUserLimit = null,
            DateTime? startsAt = null,
            DateTime? endsAt = null)
        {
            var coupon = new Coupon
            {
                RestaurantId = restaurantId,
                Code = code,
                Type = type,
                Value = value,
                MinOrderAmount = minOrder,
                MaxDiscount = maxDiscount,
                UsageLimit = usageLimit,
                PerUserLimit = perUserLimit,
                StartsAt = startsAt ?? DateTime.UtcNow.AddDays(-1),
                EndsAt = endsAt ?? DateTime.UtcNow.AddDays(1)
            };

            db.Coupons.Add(coupon);
            db.SaveChanges();
            return coupon;
        }
    }
}