using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Infrastructure.Data.Configurations
{
    public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
            builder.Property(r => r.Address).IsRequired().HasMaxLength(300);
            builder.Property(r => r.PhoneNumber).IsRequired().HasMaxLength(30);
            builder.Property(r => r.Description).HasMaxLength(1000);
            builder.Property(r => r.MinOrderAmount).HasPrecision(18, 2);
            builder.Property(r => r.DeliveryFee).HasPrecision(18, 2);

            builder.HasOne(r => r.Owner)
                .WithMany()
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
