using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
            builder.Property(c => c.Value).HasPrecision(18, 2);
            builder.Property(c => c.MinOrderAmount).HasPrecision(18, 2);
            builder.Property(c => c.MaxDiscount).HasPrecision(18, 2);

            builder.HasIndex(c => c.Code).IsUnique();

            builder.HasOne(c => c.Restaurant)
                .WithMany()
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}