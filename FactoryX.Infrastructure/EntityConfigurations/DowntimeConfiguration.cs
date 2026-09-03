using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class DowntimeConfiguration : EntityBaseConfiguration<Downtime>
{
    public override void Configure(EntityTypeBuilder<Downtime> builder)
    {
        base.Configure(builder);

        builder.ToTable("downtimes");

        builder.Property(d => d.Reason)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.StartTime)
            .IsRequired();

        builder.Property(d => d.EndTime)
            .IsRequired();

        builder.HasOne(d => d.Machine)
            .WithMany(m => m.Downtimes)
            .HasForeignKey(d => d.MachineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}