using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Infrastructure.Data.Configurations
{
     public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
            builder.HasIndex(o => o.OrderNumber).IsUnique();

            builder.Property(o => o.CustomerPhone).IsRequired().HasMaxLength(30);
            builder.Property(o => o.DeliveryAddress).HasMaxLength(300);
            builder.Property(o => o.Notes).HasMaxLength(500);
            builder.Property(o => o.CancelReason).HasMaxLength(500);

            builder.Property(o => o.Subtotal).HasPrecision(18, 2);
            builder.Property(o => o.DeliveryFee).HasPrecision(18, 2);
            builder.Property(o => o.Total).HasPrecision(18, 2);

            builder.HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
