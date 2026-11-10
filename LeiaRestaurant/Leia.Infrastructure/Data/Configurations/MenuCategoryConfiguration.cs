using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class MenuCategoryConfiguration : IEntityTypeConfiguration<MenuCategory>
    {
        public void Configure(EntityTypeBuilder<MenuCategory> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

            builder.HasQueryFilter(c => !c.IsDeleted);

            builder.HasOne(c => c.Restaurant)
                .WithMany(r => r.MenuCategories)
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}