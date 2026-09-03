using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class FinishedGoodsReleaseConfiguration : IEntityTypeConfiguration<FinishedGoodsRelease>
{
    public void Configure(EntityTypeBuilder<FinishedGoodsRelease> builder)
    {
        builder.ToTable("FinishedGoodsReleases");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReleaseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.ReleaseNumber)
            .IsUnique();

        builder.Property(r => r.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.Quantity)
            .HasPrecision(18, 4);

        builder.Property(r => r.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(r => r.TotalCost)
            .HasPrecision(18, 4);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.ProductionBatchId);
        builder.HasIndex(r => r.WarehouseId);
        builder.HasIndex(r => r.LocationId);
        builder.HasIndex(r => r.BatchNumber);

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ProductionBatch)
            .WithMany()
            .HasForeignKey(r => r.ProductionBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PackagingOrder)
            .WithMany()
            .HasForeignKey(r => r.PackagingOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.QCInspection)
            .WithMany()
            .HasForeignKey(r => r.QCInspectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Location)
            .WithMany()
            .HasForeignKey(r => r.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReleasedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReleasedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.InventoryTransaction)
            .WithMany()
            .HasForeignKey(r => r.InventoryTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
