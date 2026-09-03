using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class WasteConfiguration : IEntityTypeConfiguration<Waste>
{
    public void Configure(EntityTypeBuilder<Waste> builder)
    {
        builder.ToTable("Wastes");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WasteNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(w => w.WasteNumber)
            .IsUnique();

        builder.Property(w => w.WasteType)
            .IsRequired();

        builder.Property(w => w.Status)
            .IsRequired();

        builder.Property(w => w.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(w => w.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(w => w.UnitCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(w => w.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(w => w.RawMaterialBatchNumber)
            .HasMaxLength(100);

        builder.Property(w => w.ReasonDescription)
            .HasMaxLength(500);

        builder.Property(w => w.Notes)
            .HasMaxLength(1000);

        builder.Property(w => w.ApprovalNotes)
            .HasMaxLength(1000);

        builder.Property(w => w.ApprovalStatus)
            .HasMaxLength(50);

        // Relationships with Restrict DeleteBehavior
        builder.HasOne(w => w.ProductionBatch)
            .WithMany()
            .HasForeignKey(w => w.ProductionBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.WorkOrder)
            .WithMany()
            .HasForeignKey(w => w.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Material)
            .WithMany()
            .HasForeignKey(w => w.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Warehouse)
            .WithMany()
            .HasForeignKey(w => w.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Location)
            .WithMany()
            .HasForeignKey(w => w.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.WasteReason)
            .WithMany(r => r.Wastes)
            .HasForeignKey(w => w.WasteReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.CreatedByUser)
            .WithMany()
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ApprovedByUser)
            .WithMany()
            .HasForeignKey(w => w.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.InventoryTransaction)
            .WithMany()
            .HasForeignKey(w => w.InventoryTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
