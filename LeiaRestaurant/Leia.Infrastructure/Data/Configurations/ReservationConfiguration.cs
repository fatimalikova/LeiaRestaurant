using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.ContactPhone).IsRequired().HasMaxLength(30);
            builder.Property(r => r.Notes).HasMaxLength(500);
            builder.Property(r => r.CancelReason).HasMaxLength(500);

            builder.HasIndex(r => new { r.TableId, r.StartDateTime });
            builder.HasIndex(r => new { r.RestaurantId, r.StartDateTime });

            builder.HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Restaurant)
                .WithMany()
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}