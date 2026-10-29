using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class RestaurantCuisineConfiguration : IEntityTypeConfiguration<RestaurantCuisine>
    {
        public void Configure(EntityTypeBuilder<RestaurantCuisine> builder)
        {
            builder.HasKey(rc => new { rc.RestaurantId, rc.CuisineId });

            builder.HasOne(rc => rc.Restaurant)
                .WithMany(r => r.Cuisines)
                .HasForeignKey(rc => rc.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rc => rc.Cuisine)
                .WithMany(c => c.Restaurants)
                .HasForeignKey(rc => rc.CuisineId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}