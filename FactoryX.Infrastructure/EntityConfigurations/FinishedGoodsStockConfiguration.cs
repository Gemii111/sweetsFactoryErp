using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class FinishedGoodsStockConfiguration : IEntityTypeConfiguration<FinishedGoodsStock>
{
    public void Configure(EntityTypeBuilder<FinishedGoodsStock> builder)
    {
        builder.ToTable("FinishedGoodsStocks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(f => f.Quantity)
            .HasPrecision(18, 4);

        builder.Property(f => f.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(f => f.TotalCost)
            .HasPrecision(18, 4);

        builder.HasIndex(f => f.ProductId);
        builder.HasIndex(f => f.ProductionBatchId);
        builder.HasIndex(f => f.WarehouseId);
        builder.HasIndex(f => f.LocationId);
        builder.HasIndex(f => f.BatchNumber);

        builder.HasOne(f => f.Product)
            .WithMany()
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.ProductionBatch)
            .WithMany()
            .HasForeignKey(f => f.ProductionBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Warehouse)
            .WithMany()
            .HasForeignKey(f => f.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Location)
            .WithMany()
            .HasForeignKey(f => f.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.QCInspection)
            .WithMany()
            .HasForeignKey(f => f.QCInspectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.PackagingOrder)
            .WithMany()
            .HasForeignKey(f => f.PackagingOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
