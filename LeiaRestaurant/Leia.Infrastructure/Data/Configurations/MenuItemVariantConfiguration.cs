using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class MenuItemVariantConfiguration : IEntityTypeConfiguration<MenuItemVariant>
    {
        public void Configure(EntityTypeBuilder<MenuItemVariant> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Name).IsRequired().HasMaxLength(50);
            builder.Property(v => v.PriceDelta).HasPrecision(18, 2);

            builder.HasOne(v => v.MenuItem)
                .WithMany(m => m.Variants)
                .HasForeignKey(v => v.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}