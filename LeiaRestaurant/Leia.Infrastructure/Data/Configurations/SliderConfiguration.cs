using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Infrastructure.Data.Configurations
{
    public class SliderConfiguration : IEntityTypeConfiguration<Slider>
    {
        public void Configure(EntityTypeBuilder<Slider> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.ImageUrl).IsRequired().HasMaxLength(500);
            builder.Property(s => s.Title).HasMaxLength(150);
            builder.Property(s => s.Subtitle).HasMaxLength(250);
            builder.Property(s => s.Link).HasMaxLength(500);
        }
    }
}
