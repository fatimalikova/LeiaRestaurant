using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class AllergenConfiguration : IEntityTypeConfiguration<Allergen>
    {
        public void Configure(EntityTypeBuilder<Allergen> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
            builder.Property(a => a.IconUrl).HasMaxLength(500);

            builder.HasIndex(a => a.Name).IsUnique();
        }
    }
}