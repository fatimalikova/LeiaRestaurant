using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class RestaurantImageConfiguration : IEntityTypeConfiguration<RestaurantImage>
    {
        public void Configure(EntityTypeBuilder<RestaurantImage> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);

            builder.HasOne(i => i.Restaurant)
                .WithMany(r => r.Images)
                .HasForeignKey(i => i.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}