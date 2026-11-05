using Leia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<RestaurantImage> RestaurantImages { get; set; }
        public DbSet<Cuisine> Cuisines { get; set; }
        public DbSet<RestaurantCuisine> RestaurantCuisines { get; set; }
        public DbSet<WorkingHour> WorkingHours { get; set; }
        public DbSet<UnavailableDay> UnavailableDays { get; set; }
        public DbSet<RestaurantTable> RestaurantTables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemVariant> MenuItemVariants { get; set; }
        public DbSet<ModifierGroup> ModifierGroups { get; set; }
        public DbSet<ModifierOption> ModifierOptions { get; set; }
        public DbSet<Allergen> Allergens { get; set; }
        public DbSet<MenuItemAllergen> MenuItemAllergens { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemModifier> OrderItemModifiers { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewReply> ReviewReplies { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }
    }
}