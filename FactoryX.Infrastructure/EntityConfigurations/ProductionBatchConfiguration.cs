using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class ProductionBatchConfiguration : EntityBaseConfiguration<ProductionBatch>
{
    public override void Configure(EntityTypeBuilder<ProductionBatch> builder)
    {
        base.Configure(builder);

        builder.ToTable("production_batches");

        builder.Property(b => b.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.BatchNumber)
            .IsUnique();

        builder.Property(b => b.PlannedQuantity)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.ActualOutputQuantity)
            .HasPrecision(18, 2);

        builder.Property(b => b.OutputUnit)
            .HasMaxLength(30)
            .HasDefaultValue("KG");

        builder.Property(b => b.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(b => b.QualityStatus)
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500);

        builder.Property(b => b.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(b => b.WorkOrder)
            .WithMany(w => w.ProductionBatches)
            .HasForeignKey(b => b.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Product)
            .WithMany()
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.RecipeVersion)
            .WithMany()
            .HasForeignKey(b => b.RecipeVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ProductionLine)
            .WithMany()
            .HasForeignKey(b => b.ProductionLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.WorkCenter)
            .WithMany()
            .HasForeignKey(b => b.WorkCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Machine)
            .WithMany()
            .HasForeignKey(b => b.MachineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Operator)
            .WithMany()
            .HasForeignKey(b => b.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Shift)
            .WithMany()
            .HasForeignKey(b => b.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.TargetWarehouse)
            .WithMany()
            .HasForeignKey(b => b.TargetWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Consumptions)
            .WithOne(c => c.ProductionBatch)
            .HasForeignKey(c => c.ProductionBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.ProductionRecords)
            .WithOne(r => r.ProductionBatch)
            .HasForeignKey(r => r.ProductionBatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
