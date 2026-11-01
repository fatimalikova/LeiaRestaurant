using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class OrderItemModifierConfiguration : IEntityTypeConfiguration<OrderItemModifier>
    {
        public void Configure(EntityTypeBuilder<OrderItemModifier> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
            builder.Property(m => m.PriceDelta).HasPrecision(18, 2);

            builder.HasOne(m => m.OrderItem)
                .WithMany(i => i.Modifiers)
                .HasForeignKey(m => m.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}