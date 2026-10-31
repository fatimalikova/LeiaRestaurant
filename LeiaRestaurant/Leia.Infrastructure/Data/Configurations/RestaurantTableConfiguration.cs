using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
    {
        public void Configure(EntityTypeBuilder<RestaurantTable> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Zone).HasMaxLength(50);

            builder.HasIndex(t => new { t.RestaurantId, t.Name }).IsUnique();

            builder.HasOne(t => t.Restaurant)
                .WithMany(r => r.Tables)
                .HasForeignKey(t => t.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}