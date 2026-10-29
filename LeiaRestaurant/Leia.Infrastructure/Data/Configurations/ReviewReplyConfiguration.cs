using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leia.Infrastructure.Data.Configurations
{
    public class ReviewReplyConfiguration : IEntityTypeConfiguration<ReviewReply>
    {
        public void Configure(EntityTypeBuilder<ReviewReply> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Content).IsRequired().HasMaxLength(1000);

            builder.HasOne(r => r.Review)
                .WithOne(r => r.Reply)
                .HasForeignKey<ReviewReply>(r => r.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}