using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class MenuItemAllergenConfiguration : IEntityTypeConfiguration<MenuItemAllergen>
    {
        public void Configure(EntityTypeBuilder<MenuItemAllergen> builder)
        {
            builder.HasKey(a => new { a.MenuItemId, a.AllergenId });

            builder.HasOne(a => a.MenuItem)
                .WithMany(m => m.Allergens)
                .HasForeignKey(a => a.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Allergen)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(a => a.AllergenId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}