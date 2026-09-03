using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PackagingConsumptionConfiguration : IEntityTypeConfiguration<PackagingConsumption>
{
    public void Configure(EntityTypeBuilder<PackagingConsumption> builder)
    {
        builder.ToTable("PackagingConsumptions");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PlannedQuantity)
            .HasPrecision(18, 4);

        builder.Property(c => c.ActualQuantity)
            .HasPrecision(18, 4);

        builder.Property(c => c.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(c => c.TotalCost)
            .HasPrecision(18, 4);

        builder.Property(c => c.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.BatchNumber)
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasMaxLength(500);

        builder.HasOne(c => c.PackagingOrder)
            .WithMany(o => o.Consumptions)
            .HasForeignKey(c => c.PackagingOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Material)
            .WithMany()
            .HasForeignKey(c => c.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Warehouse)
            .WithMany()
            .HasForeignKey(c => c.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Location)
            .WithMany()
            .HasForeignKey(c => c.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.InventoryTransaction)
            .WithMany()
            .HasForeignKey(c => c.InventoryTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
