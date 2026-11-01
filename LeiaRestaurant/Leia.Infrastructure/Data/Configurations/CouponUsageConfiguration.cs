using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
    {
        public void Configure(EntityTypeBuilder<CouponUsage> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.DiscountAmount).HasPrecision(18, 2);

            builder.HasIndex(u => new { u.CouponId, u.UserId });
            builder.HasIndex(u => u.OrderId);

            builder.HasOne(u => u.Coupon)
                .WithMany()
                .HasForeignKey(u => u.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Order)
                .WithMany()
                .HasForeignKey(u => u.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}