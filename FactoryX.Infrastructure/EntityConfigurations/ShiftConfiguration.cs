using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class ShiftConfiguration : EntityBaseConfiguration<Shift>
{
    public override void Configure(EntityTypeBuilder<Shift> builder)
    {
        base.Configure(builder);

        builder.ToTable("shifts");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();
    }
}