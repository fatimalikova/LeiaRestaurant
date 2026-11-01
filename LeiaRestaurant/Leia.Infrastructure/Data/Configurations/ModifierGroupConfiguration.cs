using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class ModifierGroupConfiguration : IEntityTypeConfiguration<ModifierGroup>
    {
        public void Configure(EntityTypeBuilder<ModifierGroup> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(g => g.MenuItem)
                .WithMany(m => m.ModifierGroups)
                .HasForeignKey(g => g.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}