using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount).HasPrecision(18, 2);
            builder.Property(p => p.Provider).IsRequired().HasMaxLength(30);
            builder.Property(p => p.ProviderPaymentId).HasMaxLength(100);
            builder.Property(p => p.Brand).HasMaxLength(30);
            builder.Property(p => p.Last4).HasMaxLength(4);
            builder.Property(p => p.FailureReason).HasMaxLength(300);

            builder.HasIndex(p => p.OrderId).IsUnique();

            builder.HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}