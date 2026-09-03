using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class MachineConfiguration : EntityBaseConfiguration<Machine>
{
    public override void Configure(EntityTypeBuilder<Machine> builder)
    {
        base.Configure(builder);

        builder.ToTable("machines");

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(m => m.Capacity)
            .IsRequired();

        builder.HasMany(m => m.WorkOrders)
            .WithOne(w => w.Machine)
            .HasForeignKey(w => w.MachineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Downtimes)
            .WithOne(d => d.Machine)
            .HasForeignKey(d => d.MachineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}