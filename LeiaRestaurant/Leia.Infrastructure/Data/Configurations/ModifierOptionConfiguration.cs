using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class ModifierOptionConfiguration : IEntityTypeConfiguration<ModifierOption>
    {
        public void Configure(EntityTypeBuilder<ModifierOption> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Name).IsRequired().HasMaxLength(100);
            builder.Property(o => o.PriceDelta).HasPrecision(18, 2);

            builder.HasOne(o => o.Group)
                .WithMany(g => g.Options)
                .HasForeignKey(o => o.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}