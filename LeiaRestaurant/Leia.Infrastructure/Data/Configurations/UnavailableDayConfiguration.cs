using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Infrastructure.Data.Configurations
{
    public class UnavailableDayConfiguration : IEntityTypeConfiguration<UnavailableDay>
    {
        public void Configure(EntityTypeBuilder<UnavailableDay> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Reason).HasMaxLength(250);

            builder.HasOne(u => u.Restaurant)
                .WithMany(r => r.UnavailableDays)
                .HasForeignKey(u => u.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
