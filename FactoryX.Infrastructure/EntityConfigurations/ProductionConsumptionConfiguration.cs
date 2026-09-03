using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class ProductionConsumptionConfiguration : EntityBaseConfiguration<ProductionConsumption>
{
    public override void Configure(EntityTypeBuilder<ProductionConsumption> builder)
    {
        base.Configure(builder);

        builder.ToTable("production_consumptions");

        builder.Property(c => c.RawMaterialBatchNumber)
            .HasMaxLength(100);

        builder.Property(c => c.PlannedQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(c => c.ActualQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(c => c.Variance)
            .HasPrecision(18, 4);

        builder.Property(c => c.Unit)
            .HasMaxLength(30)
            .HasDefaultValue("KG");

        builder.Property(c => c.UnitCost)
            .HasPrecision(18, 2);

        builder.Property(c => c.TotalCost)
            .HasPrecision(18, 2);

        builder.Property(c => c.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(c => c.ProductionBatch)
            .WithMany(b => b.Consumptions)
            .HasForeignKey(c => c.ProductionBatchId)
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
            .OnDelete(DeleteBehavior.SetNull);
    }
}
