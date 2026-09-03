using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class MaterialUsageConfiguration : EntityBaseConfiguration<MaterialUsage>
{
    public override void Configure(EntityTypeBuilder<MaterialUsage> builder)
    {
        base.Configure(builder);

        builder.ToTable("material_usages");

        builder.Property(mu => mu.Quantity)
            .IsRequired();

        builder.HasOne(mu => mu.WorkOrder)
            .WithMany(w => w.MaterialUsages)
            .HasForeignKey(mu => mu.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mu => mu.Material)
            .WithMany(m => m.MaterialUsages)
            .HasForeignKey(mu => mu.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}